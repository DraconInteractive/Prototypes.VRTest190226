using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ForNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddInputPort<int>("Count");
        context.AddOutputPort("Exec");
        context.AddOutputPort("Loop");
        context.AddOutputPort<int>("Element");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new ForRTNode();
    }
}
