using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class DebugLogNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        context.AddInputPort<string>("Message");
    }

    public BaseRTNode CreateRuntimeType() => new DebugLogRTNode();
}