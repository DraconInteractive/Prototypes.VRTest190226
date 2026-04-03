using UnityEngine;

public class ToggleGameObjectRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        var objExists = TryGetInput<GameObject>("Object", graph, out var go);
        if (!objExists)
        {
            var objNameExists = TryGetInput<string>("Object", graph, out var goName);
            go = GameObject.Find(goName);
            if (!objNameExists)
            {
                Debug.LogError("Failed toggle go node");
                SetFailed();
                return;
            }
        }
        
        if (!TryGetInput<bool>("State", graph, out var state))
        {
            SetFailed();
            return;
        }
        
        Debug.Log("Suceeded toggle go node");
        go?.SetActive(state);
        DefExecNext(graph);
    }
}