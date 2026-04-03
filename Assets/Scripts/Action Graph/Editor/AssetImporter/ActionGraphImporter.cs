using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, ActionGraph.AssetExtension)]
public class ActionGraphImporter : ScriptedImporter
{
    private const string RegistryPath = "Assets/Resources/ActionGraphRegistry.asset";

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var graph = GraphDatabase.LoadGraphForImporter<ActionGraph>(ctx.assetPath);

        if (graph == null)
        {
            Debug.LogError($"Failed to load Visual Novel Director graph asset: {ctx.assetPath}");
            return;
        }

        var nodes = graph.GetNodes().ToList();

        var startNode = nodes.FirstOrDefault(x => x is StartNode);
        if (startNode == null)
        {
            Debug.LogError($"Failed to load StartNode: {ctx.assetPath}");
            return;
        }

        var runtimeGraph = new RuntimeActionGraph();
        var nodeMap = new Dictionary<INode, BaseRTNode>();

        // Pass 1: create runtime nodes, assign IDs, populate map
        int nextId = 0;
        foreach (var node in nodes)
        {
            var rt = node is IEditorNode editorNode
                ? editorNode.CreateRuntimeType()
                : new FallbackRTNode();

            rt.NodeId = (nextId++).ToString();
            nodeMap[node] = rt;

            if (node is ContextNode ctxNode && rt is BaseContextRTNode ctxRTNode)
            {
                var blocks = ctxNode.blockNodes.ToList();
                foreach (var block in blocks)
                {
                    var blockRT = block is IEditorNode edBlock
                        ? edBlock.CreateRuntimeType()
                        : new FallbackRTNode();
                    blockRT.NodeId = (nextId++).ToString();
                    nodeMap[block] = blockRT;

                    if (blockRT is BaseBlockRTNode blockRTNode)
                    {
                        blockRTNode.ContextNodeID = ctxRTNode.NodeId;
                        ctxRTNode.BlockIds.Add(blockRTNode.NodeId);
                    }
                }
            }
        }

        // Pass 2: populate ports with NodeId and default values
        foreach (var node in nodes)
        {
            var rt = nodeMap[node];
            foreach (var input in node.GetInputPorts())
            {
                var newPort = new Port { Name = input.name, Type = input.dataType, NodeId = rt.NodeId };
                if (!input.isConnected && input.TryGetValue(out object value))
                    AssignPortValue(newPort, value);
                rt.Inputs.Add(newPort);
            }

            foreach (var output in node.GetOutputPorts())
            {
                var newPort = new Port { Name = output.name, Type = output.dataType, NodeId = rt.NodeId };
                if (!output.isConnected && output.TryGetValue(out object value))
                    AssignPortValue(newPort, value);
                rt.Outputs.Add(newPort);
            }

            if (node is ContextNode ctxNode)
            {
                foreach (var block in ctxNode.blockNodes)
                {
                    var blockRT = nodeMap[block];
                    foreach (var input in block.GetInputPorts())
                    {
                        var newPort = new Port { Name = input.name, Type = input.dataType, NodeId = blockRT.NodeId };
                        if (!input.isConnected && input.TryGetValue(out object value))
                            AssignPortValue(newPort, value);
                        blockRT.Inputs.Add(newPort);
                    }
                    foreach (var output in block.GetOutputPorts())
                    {
                        var newPort = new Port { Name = output.name, Type = output.dataType, NodeId = blockRT.NodeId };
                        if (!output.isConnected && output.TryGetValue(out object value))
                            AssignPortValue(newPort, value);
                        blockRT.Outputs.Add(newPort);
                    }
                }
            }
        }

        // Pass 3: wire connections as "nodeId__portName" strings
        foreach (var node in nodes)
        {
            var rt = nodeMap[node];
            foreach (var output in node.GetOutputPorts())
            {
                if (!output.isConnected) continue;

                var rtOutputPort = rt.Outputs.First(x => x.Name == output.name);
                var connectedInputs = new List<IPort>();
                output.GetConnectedPorts(connectedInputs);

                foreach (var connection in connectedInputs)
                {
                    var targetRT = nodeMap[connection.GetNode()];
                    var targetRTPort = targetRT.Inputs.First(x => x.Name == connection.name);

                    rtOutputPort.Connections.Add($"{targetRT.NodeId}__{targetRTPort.Name}");
                    targetRTPort.Connections.Add($"{rt.NodeId}__{rtOutputPort.Name}");
                }
            }

            if (node is ContextNode ctxNode)
            {
                foreach (var block in ctxNode.blockNodes)
                {
                    var blockRT = nodeMap[block];
                    foreach (var output in block.GetOutputPorts())
                    {
                        if (!output.isConnected) continue;

                        var rtOutputPort = rt.Outputs.First(x => x.Name == output.name);
                        var connectedinputs = new List<IPort>();
                        output.GetConnectedPorts(connectedinputs);

                        foreach (var connection in connectedinputs)
                        {
                            var targetRT = nodeMap[connection.GetNode()];
                            var targetRTPort = targetRT.Inputs.First(x => x.Name == connection.name);
                            
                            rtOutputPort.Connections.Add($"{targetRT.NodeId}__{targetRTPort.Name}");
                            targetRTPort.Connections.Add($"{rt.NodeId}__{rtOutputPort.Name}");
                        }
                    }
                }
            }
        }

        runtimeGraph.Nodes = nodeMap.Values.ToList();

        // Pass 4: populate LoopBodyNodeIds for loop nodes
        foreach (var node in runtimeGraph.Nodes.OfType<BaseLoopRTNode>())
        {
            var loopPort = node.Outputs.FirstOrDefault(p => p.Name == "Loop");
            if (loopPort == null || loopPort.Connections.Count == 0) continue;

            var visited = new HashSet<string>();
            var queue = new Queue<string>();

            runtimeGraph.ResolveInputConnection(loopPort.Connections[0], out var seed, out _);
            if (seed != null) queue.Enqueue(seed.NodeId);

            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!visited.Add(id)) continue;

                var n = runtimeGraph.GetNodeById(id);
                if (n == null) continue;

                var execOut = n.Outputs.FirstOrDefault(p => p.Name == "Exec");
                if (execOut != null)
                {
                    foreach (var conn in execOut.Connections)
                    {
                        runtimeGraph.ResolveInputConnection(conn, out var next, out _);
                        if (next != null && !visited.Contains(next.NodeId))
                            queue.Enqueue(next.NodeId);
                    }
                }
            }

            node.LoopBodyNodeIds = visited.ToList();
        }

        var eGraphName = Path.GetFileName(ctx.assetPath);
        var assetName = Path.GetFileNameWithoutExtension(ctx.assetPath) + "_rt";
        var assetPath = ctx.assetPath.Replace(eGraphName, assetName + ".asset");
        var serialized = runtimeGraph.Serialize();

        // Create or update the ActionGraphAsset
        var graphAsset = AssetDatabase.LoadAssetAtPath<ActionGraphAsset>(assetPath);
        if (graphAsset != null)
        {
            graphAsset.SerializedGraph = serialized;
            EditorUtility.SetDirty(graphAsset);
        }
        else
        {
            graphAsset = ScriptableObject.CreateInstance<ActionGraphAsset>();
            graphAsset.name = assetName;
            graphAsset.SerializedGraph = serialized;
            AssetDatabase.CreateAsset(graphAsset, assetPath);
        }

        RegisterInRegistry(graphAsset);
    }

    private static void RegisterInRegistry(ActionGraphAsset graphAsset)
    {
        // Find or create the registry
        var registry = AssetDatabase.LoadAssetAtPath<ActionGraphRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<ActionGraphRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }
        
        // Add to registry if not already present
        registry.Graphs.RemoveAll(g => g == null);
        if (registry.Graphs.All(g => g.name != graphAsset.name))
        {
            registry.Graphs.Add(graphAsset);
            EditorUtility.SetDirty(registry);
        }

    }

    // Unity assets can't be serialized by Json.NET — store them as their asset name (string) instead.
    // The runtime resolves the name back to the asset via a registry (e.g. AudioController.GetClip).
    private static void AssignPortValue(Port port, object value)
    {
        if (value is UnityEngine.Object unityObj && AssetDatabase.Contains(unityObj))
        {
            port.Value = unityObj.name;
            port.Type = typeof(string);
        }
        else
        {
            port.Value = value;
        }
    }
}
