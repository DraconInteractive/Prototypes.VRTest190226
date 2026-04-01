public class FindBowRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        TrySetOutput("Bow", BowController.Instance);
    }
}