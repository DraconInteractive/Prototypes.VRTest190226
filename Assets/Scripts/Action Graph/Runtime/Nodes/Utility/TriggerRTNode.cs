public class TriggerRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<string>("Id", graph, out var id))
        {
            SetFailed();
            return;
        }

        graph.OnTrigger?.Invoke(id);
        DefExecNext(graph);
    }
}
