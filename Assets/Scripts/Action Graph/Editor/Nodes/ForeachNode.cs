using System;
using Unity.GraphToolkit.Editor;

public class ForeachNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddInputPort<object[]>("Array");
        context.AddOutputPort("Exec");
        context.AddOutputPort("Loop");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new FallbackRTNode();
    }
}