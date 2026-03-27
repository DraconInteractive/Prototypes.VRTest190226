using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class SpawnWaveNode : BaseContextActionNode, IEditorNode
{
    public BaseRTNode CreateRuntimeType()
    {
        return new SpawnWaveRTNode();
    }
}

[UseWithContext(typeof(SpawnWaveNode))]
[Serializable]
public class SpawnMonsterBlockNode : BlockNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<int>("Count").WithDefaultValue(1);
        context.AddInputPort<SpawnMonsterRTBlockNode.MonsterType>("Prefab");
        context.AddInputPort<Transform[]>("Spawn Points");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new SpawnMonsterRTBlockNode();
    }
}

[UseWithContext(typeof(SpawnWaveNode))]
[Serializable]
public class RewardBlockNode : BlockNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<int>("Gold").WithDefaultValue(1);
        context.AddInputPort<int>("XP").WithDefaultValue(1);
        context.AddInputPort<GameObject[]>("Items");
    }

    public BaseRTNode CreateRuntimeType()
    {
        return new RewardBlockRTNode();
    }
}