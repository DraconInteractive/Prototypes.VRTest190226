public class FindBowRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        TrySetOutput("Bow", BowController.Instance);
    }
}