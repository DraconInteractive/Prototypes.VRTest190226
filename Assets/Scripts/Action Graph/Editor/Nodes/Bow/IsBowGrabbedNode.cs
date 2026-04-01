using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class IsBowGrabbedNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<BowController>("Bow");
        context.AddOutputPort<bool>("IsGrabbed");
    }

    public BaseRTNode CreateRuntimeType() => new IsBowGrabbedRTNode();
}