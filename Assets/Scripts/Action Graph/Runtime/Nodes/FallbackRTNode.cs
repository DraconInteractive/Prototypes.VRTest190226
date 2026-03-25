using UnityEngine;

public class FallbackRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        Debug.LogWarning("FallbackRTNode executed — no runtime implementation for this node type.");
        DefExecNext(graph);
    }
}
