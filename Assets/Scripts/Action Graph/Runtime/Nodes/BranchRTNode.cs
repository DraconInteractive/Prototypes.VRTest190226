public class BranchRTNode : BaseRTNode
{
    protected override void ExecuteInternal()
    {
        if (!TryGetInput<bool>("Condition", out bool condition))
        {
            SetFailed();
            return;
        }

        SetComplete();

        var execOutput = GetOutputPort(condition ? "True" : "False");
        execOutput?.ConnectedPorts[0]?.Node.Execute();
    }
}
