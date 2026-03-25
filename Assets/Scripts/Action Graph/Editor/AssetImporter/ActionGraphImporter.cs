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

                    // Output port stores a connection pointing to the input port's node+name
                    rtOutputPort.Connections.Add($"{targetRT.NodeId}__{targetRTPort.Name}");
                    // Input port stores a connection pointing back to this output port's node+name
                    targetRTPort.Connections.Add($"{rt.NodeId}__{rtOutputPort.Name}");
                }
            }
        }

        runtimeGraph.Nodes = nodeMap.Values.ToList();

        var eGraphName = Path.GetFileName(ctx.assetPath);
        var assetName = Path.GetFileNameWithoutExtension(ctx.assetPath) + "_rt";
        var path = ctx.assetPath.Replace(eGraphName, assetName + ".asset");

        var serialized = runtimeGraph.Serialize();

        var existing = AssetDatabase.LoadAssetAtPath<ActionGraphAsset>(path);
        if (existing != null)
        {
            existing.SerializedGraph = serialized;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var graphAsset = ScriptableObject.CreateInstance<ActionGraphAsset>();
            graphAsset.name = assetName;
            graphAsset.SerializedGraph = serialized;
            AssetDatabase.CreateAsset(graphAsset, path);
        }
    }
}
