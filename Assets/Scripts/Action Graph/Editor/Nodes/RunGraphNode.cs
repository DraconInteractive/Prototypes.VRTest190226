using System;
using UnityEngine;

[Serializable]
public class RunGraphNode : BaseActionNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<ActionGraph>("Graph");
        context.AddInputPort<bool>("Wait?").WithDefaultValue(true);
    }
}
