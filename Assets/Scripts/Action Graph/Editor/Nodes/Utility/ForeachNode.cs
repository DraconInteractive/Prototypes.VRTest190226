using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ForeachNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddInputPort<object[]>("Array");
        context.AddOutputPort("Exec");
        context.AddOutputPort("Loop");
        context.AddOutputPort<object>("Element");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new ForeachRTNode();
    }
}