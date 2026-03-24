using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
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

        var nodes = graph.GetNodes();
        
        var startNode = nodes.FirstOrDefault(x => x is StartNode);
        if (startNode == null)
        {
            Debug.LogError($"Failed to load StartNode: {ctx.assetPath}");
            return;
        }

        var runtimeGraph = new RuntimeActionGraph();
        
        // First, create the runtime variants of all nodes and add them to the runtime graph
        // Create a hashmap of map[editor]=runtime and populate that when you do the above
        // Then, loop back through the nodes to update ports
        // E.g, foreach port in outputs, get connected ports
        // Get the node owner of the other port, and create that connection for both nodes
        // Since every connected port output needs an input, you shouldnt need to iterate inputs separately
        // Make sure to also update node values, which is important for nodes to get unconnected default values
        
        // Some nodes are connected to billboard values. Investigate how that works
        
        Dictionary<INode, BaseRTNode> nodeMap = new Dictionary<INode, BaseRTNode>();
        
        foreach (var node in nodes)
        {
            // 1. Resolve runtime and populate map
            
            // Try and find matching runtime type
            // e.g if node is BaseNode, get BaseNode.RuntimeType
            // Use a default node type for any non-compliant nodes. These should have their ports etc populated since the runtime wont be set up
            if (node is IEditorNode editorNode)
            {
                nodeMap[node] = editorNode.CreateRuntimeType();
            }
            else
            {
                nodeMap[node] = new FallbackRTNode();
            }
        }
        
        // Populate ports
        // This could be done manually in definition, but fuck that
        foreach (var node in nodes)
        {
            var rt = nodeMap[node];
            foreach (var input in node.GetInputPorts())
            {
                var newPort = new Port()
                {
                    Name = input.name,
                    Type = input.dataType,
                    Node = rt
                };
                if (!input.isConnected && input.TryGetValue(out object value))
                {
                    newPort.Value = value;
                }

                rt.Inputs.Add(newPort);
            }

            foreach (var output in node.GetOutputPorts())
            {
                var newPort = new Port()
                {
                    Name = output.name,
                    Type = output.dataType,
                    Node = rt
                };
                if (!output.isConnected && output.TryGetValue(out object value))
                {
                    newPort.Value = value;
                }
                
                rt.Outputs.Add(newPort);
            }
        }
        
        foreach (var node in nodes)
        {
            var rt = nodeMap[node];
            // 2. Resolve ports
            foreach (var output in node.GetOutputPorts())
            {
                var rtPort = rt.Outputs.First(x => x.Name == output.name);
                
                if (output.isConnected)
                {
                    var connectedInputs = new List<IPort>();
                    output.GetConnectedPorts(connectedInputs);
                    foreach (var connection in connectedInputs)
                    {
                        var targetRT = nodeMap[connection.GetNode()];
                        var targetRTPort = targetRT.Inputs.First(x => x.Name == connection.name);
                        // Add connection to this ports (output) list
                        rtPort.ConnectedPorts.Add(targetRTPort);
                        // Add connection to this ports (input) list
                        targetRTPort.ConnectedPorts.Add(rtPort);
                    }
                }
                else
                {
                    
                }
            }

            foreach (var input in node.GetInputPorts())
            {
                
            }
        }
    }
}
