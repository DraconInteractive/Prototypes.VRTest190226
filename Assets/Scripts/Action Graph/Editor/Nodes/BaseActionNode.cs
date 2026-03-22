using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class BaseActionNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("Exec").Build();
        context.AddOutputPort("Exec").Build();
    }
}
