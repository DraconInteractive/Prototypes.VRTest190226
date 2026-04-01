using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForeachRTNode : BaseLoopRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<object[]>("Array", graph, out var values))
        {
            SetFailed();
            return;
        }

        var loopPort = GetOutputPort("Loop");
        if (loopPort == null || loopPort.Connections.Count == 0)
        {
            DefExecNext(graph);
            return;
        }

        graph.ResolveInputConnection(loopPort.Connections[0], out var firstLoopNode, out _);
        graph.CoroutineRunner.StartCoroutine(RunLoop(graph, values, firstLoopNode));
    }

    private IEnumerator RunLoop(RuntimeActionGraph graph, object[] values, BaseRTNode firstLoopNode)
    {
        foreach (var element in values)
        {
            foreach (var id in LoopBodyNodeIds)
                graph.GetNodeById(id)?.Reset(graph);

            TrySetOutput("Element", element);
            firstLoopNode.Execute(graph);

            yield return new WaitUntil(() => LoopBodyNodeIds.All(id =>
            {
                var n = graph.GetNodeById(id);
                return n == null
                    || n.State == NodeState.Complete
                    || n.State == NodeState.Failed;
            }));
        }

        DefExecNext(graph);
    }
}
