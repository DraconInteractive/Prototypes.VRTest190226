using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class TriggerNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        context.AddInputPort<string>("Id");
    }

    public BaseRTNode CreateRuntimeType() => new TriggerRTNode();
}
