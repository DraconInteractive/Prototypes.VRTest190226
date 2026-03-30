using UnityEngine;

public class DebugLogRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<string>("Message", graph, out string message))
        {
            SetFailed();
            return;
        }

        Debug.Log(message);
        DefExecNext(graph);
    }
}
