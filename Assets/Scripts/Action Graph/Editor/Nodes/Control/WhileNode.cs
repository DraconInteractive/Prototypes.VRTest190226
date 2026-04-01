using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class WhileNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        
        context.AddInputPort<bool>("Condition");
        context.AddOutputPort("Loop");
    }

    public BaseRTNode CreateRuntimeType() => new WhileRTNode();
}