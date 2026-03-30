using UnityEngine;

public class ForeachRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Array", graph, out object[] values))
        {
            Debug.LogError("No array provided to foreach node");
            SetFailed();
            return;
        }

        var loopOutput = GetOutputPort("Loop");
        if (loopOutput == null || loopOutput.Connections.Count == 0)
        {
            DefExecNext(graph);
            return;
        }
        
        graph.ResolveInputConnection(loopOutput.Connections[0], out var nextNode, out _);
        for (int i = 0; i < values.Length; i++)
        {
            TrySetOutput("Element", values[i]);
            nextNode?.Execute(graph);
        }
        
        SetComplete();
        DefExecNext(graph);
    }
}