using UnityEngine;

public class ToggleGameObjectRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<GameObject>("Object", graph, out var go) || !TryGetInput<bool>("State", graph, out var state))
        {
            SetFailed();
            return;
        }

        go.SetActive(state);
        DefExecNext(graph);
    }
}