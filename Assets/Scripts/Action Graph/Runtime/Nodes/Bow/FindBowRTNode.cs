public class FindBowRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (BowController.Instance == null)
        {
            graph.PrintDebugError(this, "No bow present in scene");
        }
        TrySetOutput("Bow", BowController.Instance);
    }
}