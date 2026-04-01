using System.Collections;
using Audio;
using UnityEngine;

namespace Action_Graph.Runtime.Nodes.Audio
{
    public class PlayAdvancedVoiceOverRTNode : BaseRTNode
    {
        protected override void ExecuteInternal(RuntimeActionGraph graph)
        {
            if (TryGetInput<string>("Id", graph, out var id) ||
                !TryGetInput<AudioController.VOSpeaker>("Speaker", graph, out var speaker) ||
                !TryGetInput<AudioClip>("Clip", graph, out var clip) ||
                !TryGetInput<AudioSource>("Source", graph, out var source) ||
                !TryGetInput<bool>("Clear Queue?", graph, out var clearQueue) ||
                !TryGetInput<bool>("Wait?", graph, out var wait))
            {
                SetFailed();
                return;
            }

            var state = AudioController.Instance.AddVoiceOver(speaker, clip, id, source, clearQueue: clearQueue);
            if (wait)
            {
                graph.CoroutineRunner.StartCoroutine(WaitRoutine(graph, state));
            }
            else
            {
                DefExecNext(graph);
            }
        }
        
        private IEnumerator WaitRoutine(RuntimeActionGraph graph, AudioController.VOState state)
        {
            bool finished = false;
            state.OnComplete += s => finished = true;
            state.OnCancel += s => finished = true;
        
            graph.PrintDebug(this, "Waiting for VO...");
            while (!finished)
            {
                yield return null;
            }
            graph.PrintDebug(this, "VO finished");
            DefExecNext(graph);
            yield break;
        }
    }
}