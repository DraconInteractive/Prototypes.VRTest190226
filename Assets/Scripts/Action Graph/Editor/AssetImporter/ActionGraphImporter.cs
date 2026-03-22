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
        
        foreach (var node in nodes)
        {
            // 1. Get runtime version
            // 2. Add to runtime graph nodes
            
            // 3. Instead of storing pointer to node in output, convert the output to an int that resolves to the index of the correct node
            //    Either that, or put in another loop once this is done and resolve the index to the runtime node instead
            foreach (var output in node.GetOutputPorts())
            {
                
            }

            foreach (var input in node.GetInputPorts())
            {
                
            }
        }
    }
}
