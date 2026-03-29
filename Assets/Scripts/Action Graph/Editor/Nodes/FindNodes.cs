using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class FindGameObjectByName : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<string>("Name");
        context.AddOutputPort<GameObject>("Object");
    }
    
    public BaseRTNode CreateRuntimeType()
    {
        return new FindGameObjectByNameRTNode();
    }
}

[Serializable]
public class FindGameObjectsByTag : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        context.AddInputPort<string>("Tag");
        context.AddOutputPort<GameObject[]>("Objects");
    }
    
    public BaseRTNode CreateRuntimeType()
    {
        return new FindGameObjectsByTagRTNode();
    }
}

[Serializable]
public class FindEnemiesByName : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);
        
        context.AddInputPort<string>("Name");
        context.AddOutputPort<TestEnemy[]>("Enemies");
    }
    
    public BaseRTNode CreateRuntimeType()
    {
        return new FindEnemiesByNameRTNode();
    }
}

[Serializable]
public class FindTargetsByName : Node, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<string>("Name");
        context.AddOutputPort<Target[]>("Targets").Build();
    }
    
    public BaseRTNode CreateRuntimeType()
    {
        return new FindTargetsByNameRTNode();
    }
}