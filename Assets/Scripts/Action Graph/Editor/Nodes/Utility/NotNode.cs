using Unity.GraphToolkit.Editor;

public class NotNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<bool>("In");
        context.AddOutputPort<bool>("Out");
    }

    public BaseRTNode CreateRuntimeType() => new NotRTNode();
}