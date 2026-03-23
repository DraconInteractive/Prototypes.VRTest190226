using Unity.GraphToolkit.Editor;
using System;

public abstract class BaseContextActionNode : ContextNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddOutputPort("Exec");
    }
}