using System.Collections.Generic;
using UnityEngine;

public class SpawnWaveRTNode : BaseContextRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        Debug.Log($"Executing Spawn Wave {BlockIds.Count} blocks");
        foreach (var blockId in BlockIds)
        {
            var block = graph.GetNodeById(blockId);
            block.Execute(graph);
        }
        DefExecNext(graph);
    }
}

public class SpawnMonsterRTBlockNode : BaseBlockRTNode
{
    public enum MonsterType
    {
        Wolf,
        Demon,
        Angel
    }

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<int>("Count", graph, out var count))
        {
            SetFailed();
            return;
        }
        
        if (!TryGetInput<MonsterType>("Prefab", graph, out var monsterType))
        {
            SetFailed();
            return;
        }
        graph.PrintDebug(this, $"Spawning {count} {monsterType}s");
    }
}

public class RewardBlockRTNode : BaseBlockRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<int>("Gold", graph, out var g) || !TryGetInput<int>("XP", graph, out var xp))
        {
            SetFailed();
            return;
        }

        graph.PrintDebug(this,$"Awarding {g} gold and {xp} XP");
    }
}
