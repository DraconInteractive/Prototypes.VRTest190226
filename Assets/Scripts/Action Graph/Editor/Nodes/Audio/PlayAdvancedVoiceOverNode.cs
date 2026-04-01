using Action_Graph.Runtime.Nodes.Audio;
using Audio;
using UnityEngine;

public class PlayAdvancedVoiceOverNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<string>("Id");
        context.AddInputPort<AudioController.VOSpeaker>("Speaker");
        context.AddInputPort<AudioClip>("Clip");
        context.AddInputPort<AudioSource>("Source");
        context.AddInputPort<bool>("Clear Queue?");
        context.AddInputPort<bool>("Wait?");
    }

    public BaseRTNode CreateRuntimeType() => new PlayAdvancedVoiceOverRTNode();
}