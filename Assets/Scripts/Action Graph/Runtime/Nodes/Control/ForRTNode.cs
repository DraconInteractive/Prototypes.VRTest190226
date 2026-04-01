using System.Collections;
using System.Linq;
using UnityEngine;

public class ForRTNode : BaseLoopRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<int>("Count", graph, out var count))
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
        graph.CoroutineRunner.StartCoroutine(RunLoop(graph, count, firstLoopNode));
    }

    private IEnumerator RunLoop(RuntimeActionGraph graph, int count, BaseRTNode firstLoopNode)
    {
        for (var i = 0; i < count; i++)
        {
            foreach (var id in LoopBodyNodeIds)
                graph.GetNodeById(id)?.Reset(graph);

            TrySetOutput("Element", i);
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
