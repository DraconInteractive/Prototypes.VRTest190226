using UnityEngine;

public class IsBowGrabbedRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Bow", graph, out BowController bow))
        {
            Debug.LogError("No bow input provided to IsGrabbed node");
            SetFailed();
            return;
        }

        TrySetOutput("IsGrabbed", bow.InHand);
    }
}