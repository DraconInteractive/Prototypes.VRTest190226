using System;
using UnityEngine;

[Serializable]
public class ToggleGameObjectNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<GameObject>("Object");
        context.AddInputPort<bool>("State");
    }

    public BaseRTNode CreateRuntimeType() => new ToggleGameObjectRTNode();
}