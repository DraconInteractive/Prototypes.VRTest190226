public class RunGraphRTNode : BaseRTNode
{
    protected override void ExecuteInternal()
    {
        // TODO: read "Graph" (ActionGraph) and "Wait?" (bool) inputs, then execute the sub-graph
        // If Wait? is true, this node should remain Running until the sub-graph completes
        DefExecNext();
    }
}
