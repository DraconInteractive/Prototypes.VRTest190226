using Unity.GraphToolkit.Editor;

public class ToStringNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<object>("Target");
        context.AddOutputPort<string>("String");
    }

    public BaseRTNode CreateRuntimeType() => new ToStringRTNode();
}