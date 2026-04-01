using UnityEngine;

public class ToStringRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Target", graph, out object targetObj))
        {
            SetFailed();
            return;
        }

        TrySetOutput("String", targetObj.ToString());
        SetComplete();
    }
}