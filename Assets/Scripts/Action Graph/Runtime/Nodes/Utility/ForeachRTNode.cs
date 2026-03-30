using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForeachRTNode : BaseRTNode
{
    public List<string> LoopBodyNodeIds = new();

    public override void Reset(RuntimeActionGraph graph)
    {
        base.Reset(graph);
        foreach (var nodeId in LoopBodyNodeIds)
        {
            graph.GetNodeById(nodeId).Reset(graph);
        }
    }

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<object[]>("Array", graph, out var values))
        {
            Debug.LogError("No array provided to foreach node");
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
            // Reset all loop body nodes to Idle before each iteration
            foreach (var id in LoopBodyNodeIds)
                graph.GetNodeById(id)?.Reset(graph);

            TrySetOutput("Element", element);
            firstLoopNode.Execute(graph);

            // Wait for all loop body nodes to reach a terminal state
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
