using System.Collections;
using System.Linq;
using UnityEngine;

public class WhileRTNode : BaseLoopRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<bool>("Condition", graph, out bool initState))
        {
            Debug.LogError("No condition provided to while node");
            SetFailed();
            return;
        }

        if (!initState)
        {
            DefExecNext(graph);
            return;
        }
        
        var loopPort = GetOutputPort("Loop");
        if (loopPort == null || loopPort.Connections.Count == 0)
        {
            DefExecNext(graph);
            return;
        }

        graph.ResolveInputConnection(loopPort.Connections[0], out var firstLoopNode, out _);
        graph.CoroutineRunner.StartCoroutine(RunLoop(graph, firstLoopNode));
    }

    private IEnumerator RunLoop(RuntimeActionGraph graph, BaseRTNode firstLoopNode)
    {
        bool state = true;

        while (state)
        {
            // Update condition state
            if (!TryGetInput<bool>("Condition", graph, out state))
            {
                Debug.LogError("No condition provided to while node");
                SetFailed();
                break;
            }
            
            foreach (var id in LoopBodyNodeIds)
                graph.GetNodeById(id)?.Reset(graph);
            
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