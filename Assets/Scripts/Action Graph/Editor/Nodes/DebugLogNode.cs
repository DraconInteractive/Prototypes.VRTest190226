using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class DebugLogNode : BaseActionNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        context.AddInputPort<string>("Message");
    }
}