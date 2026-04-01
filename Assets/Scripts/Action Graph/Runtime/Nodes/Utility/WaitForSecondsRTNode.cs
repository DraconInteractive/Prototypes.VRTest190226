using System.Collections;
using UnityEngine;

public class WaitForSecondsRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Seconds", graph, out float seconds))
        {
            Debug.LogError("Failed to get seconds for wait for seconds node");
            SetFailed();
            return;
        }
        
        graph.CoroutineRunner.StartCoroutine(Wait(graph, seconds));

    }

    private IEnumerator Wait(RuntimeActionGraph graph, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DefExecNext(graph);
    }
}