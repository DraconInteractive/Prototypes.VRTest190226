using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ForeachNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        context.AddInputPort<object[]>("Array");
        context.AddOutputPort("Loop");
        context.AddOutputPort<object>("Element");
    }

    public BaseRTNode CreateRuntimeType() => new ForeachRTNode();
}