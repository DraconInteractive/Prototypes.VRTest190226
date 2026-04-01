using UnityEngine;

public class NotRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("In", graph, out bool input))
        {
            SetFailed();
            return;
        }

        TrySetOutput("Out", !input);
    }
}