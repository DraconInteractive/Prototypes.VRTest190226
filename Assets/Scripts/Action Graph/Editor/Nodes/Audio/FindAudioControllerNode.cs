using Audio;
using Unity.GraphToolkit.Editor;

public class FindAudioControllerNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort<AudioController>("Audio Controller");
    }

    public BaseRTNode CreateRuntimeType() => new FindAudioControllerRTNode();
}