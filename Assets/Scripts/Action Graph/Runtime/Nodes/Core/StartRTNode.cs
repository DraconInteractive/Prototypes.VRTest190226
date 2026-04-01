public class StartRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<bool>("Debug?", graph, out var value))
        {
            graph.ShowDebug = value;
        }
        DefExecNext(graph);
    }
}
