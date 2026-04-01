using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class FirstNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<object[]>("Array");
        context.AddOutputPort<object>("First");
    }

    public BaseRTNode CreateRuntimeType() => new FirstRTNode();
}