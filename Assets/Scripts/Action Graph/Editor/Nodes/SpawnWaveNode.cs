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
public class SpawnMonsterBlockNode : BlockNode
{
    public enum MonsterType
    {
        Wolf,
        Demon,
        Angel
    }
    
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<int>("Count").WithDefaultValue(1);
        context.AddInputPort<MonsterType>("Prefab");
        context.AddInputPort<Transform[]>("Spawn Points");
    }
}

[UseWithContext(typeof(SpawnWaveNode))]
[Serializable]
public class RewardBlockNode : BlockNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<int>("Gold").WithDefaultValue(1);
        context.AddInputPort<int>("XP").WithDefaultValue(1);
        context.AddInputPort<GameObject[]>("Items");
    }
}