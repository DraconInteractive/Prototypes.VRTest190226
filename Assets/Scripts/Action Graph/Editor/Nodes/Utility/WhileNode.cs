using Unity.GraphToolkit.Editor;

public class WhileNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddOutputPort("Exec");
        context.AddInputPort<bool>("Condition");
        context.AddOutputPort("Loop");
    }

    public BaseRTNode CreateRuntimeType() => new WhileRTNode();
}