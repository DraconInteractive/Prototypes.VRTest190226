using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("Exec");
    }
}
