public class BranchRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<bool>("Condition", graph, out bool condition))
        {
            SetFailed();
            return;
        }

        SetComplete();

        var execOutput = GetOutputPort(condition ? "True" : "False");
        if (execOutput == null || execOutput.Connections.Count == 0) return;
        graph.ResolveInputConnection(execOutput.Connections[0], out var nextNode, out _);
        nextNode?.Execute(graph);
    }
}
