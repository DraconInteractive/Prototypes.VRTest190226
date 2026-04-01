using UnityEngine;

public class NotRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("In", graph, out bool input))
        {
            Debug.LogError("Couldn't get input for Not node");
            SetFailed();
            return;
        }

        TrySetOutput("Out", !input);
    }
}