using UnityEngine;

public class FallbackRTNode : BaseRTNode
{
    protected override void ExecuteInternal()
    {
        Debug.LogWarning("FallbackRTNode executed — no runtime implementation for this node type.");
        SetComplete();
    }
}
