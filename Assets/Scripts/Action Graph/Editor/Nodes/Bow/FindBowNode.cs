using Unity.GraphToolkit.Editor;

public class FindBowNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort<BowController>("Bow");
    }

    public BaseRTNode CreateRuntimeType() => new FindBowRTNode();
}