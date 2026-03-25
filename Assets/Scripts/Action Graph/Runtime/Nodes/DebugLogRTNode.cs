using UnityEngine;

public class DebugLogRTNode : BaseRTNode
{
    protected override void ExecuteInternal()
    {
        if (!TryGetInput<string>("Message", out string message))
        {
            SetFailed();
            return;
        }

        Debug.Log(message);
        DefExecNext();
    }
}
