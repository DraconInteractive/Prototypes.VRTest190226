using Audio;

public class FindAudioControllerRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        var controller = AudioController.Instance;
        TrySetOutput("Audio Controller", controller);
    }
}