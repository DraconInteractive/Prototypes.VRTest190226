using Audio;
using UnityEngine;

public class PlayBasicVoiceOverNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<AudioController.VOSpeaker>("Speaker");
        context.AddInputPort<AudioClip>("Clip");
        context.AddInputPort<bool>("Clear Queue?");
        context.AddInputPort<bool>("Wait?");
    }

    public BaseRTNode CreateRuntimeType() => new PlayBasicVoiceOverRTNode();
}