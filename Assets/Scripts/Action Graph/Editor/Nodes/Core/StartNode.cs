using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class StartNode : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("Exec");
        context.AddInputPort<bool>("Debug?");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new StartRTNode();
    }
}
