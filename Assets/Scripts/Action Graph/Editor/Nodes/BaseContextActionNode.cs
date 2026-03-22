using Unity.GraphToolkit.Editor;
using System;

[Serializable]
public class BaseContextActionNode : ContextNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddOutputPort("Exec");
    }
}