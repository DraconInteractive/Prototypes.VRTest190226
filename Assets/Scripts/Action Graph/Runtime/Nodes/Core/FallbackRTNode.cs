using UnityEngine;

public class FallbackRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        graph.PrintDebug(this, "Fallback executed, review runtime implementation of this node");
        DefExecNext(graph);
    }
}
