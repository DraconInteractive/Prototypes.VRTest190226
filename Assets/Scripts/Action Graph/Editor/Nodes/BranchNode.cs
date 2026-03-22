using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class BranchNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec");
        context.AddInputPort<bool>("Condition");
        context.AddOutputPort("True");
        context.AddOutputPort("False");
    }
}