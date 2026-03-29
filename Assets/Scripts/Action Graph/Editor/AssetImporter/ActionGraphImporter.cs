using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, ActionGraph.AssetExtension)]
public class ActionGraphImporter : ScriptedImporter
{
    private const string RegistryPath = "Assets/Data/ActionGraphRegistry.asset";

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
                    newPort.Value = value;
                rt.Inputs.Add(newPort);
            }

            foreach (var output in node.GetOutputPorts())
            {
                var newPort = new Port { Name = output.name, Type = output.dataType, NodeId = rt.NodeId };
                if (!output.isConnected && output.TryGetValue(out object value))
                    newPort.Value = value;
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
                        {
                            newPort.Value = value;
                        }

                        blockRT.Inputs.Add(newPort);
                    }
                    foreach (var output in block.GetOutputPorts())
                    {
                        var newPort = new Port { Name = output.name, Type = output.dataType, NodeId = blockRT.NodeId };
                        if (!output.isConnected && output.TryGetValue(out object value))
                        {
                            newPort.Value = value;
                        }

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

        // Defer Addressables registration — CreateOrMoveEntry internally calls
        // SerializeState → AssetDatabase.SaveAssets, which is restricted during import.
        EditorApplication.delayCall += EnsureRegistryAddressable;
    }

    private static void EnsureRegistryAddressable()
    {
        var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
        if (addressableSettings == null)
        {
            Debug.LogWarning("ActionGraphImporter: Addressable Asset Settings not found. Create an Addressables configuration to enable runtime graph loading.");
            return;
        }

        var guid = AssetDatabase.AssetPathToGUID(RegistryPath);
        var entry = addressableSettings.FindAssetEntry(guid)
            ?? addressableSettings.CreateOrMoveEntry(guid, addressableSettings.DefaultGroup);
        entry.address = ActionGraphManager.RegistryAddress;
    }
}
