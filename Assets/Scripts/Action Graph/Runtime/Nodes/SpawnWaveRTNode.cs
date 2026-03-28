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
        // TODO: read block data (monster types, counts, spawn points) and trigger wave spawning
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
            Debug.LogError("Failed to get # monsters to spawn");
            SetFailed();
            return;
        }
        
        if (!TryGetInput<MonsterType>("Prefab", graph, out var monsterType))
        {
            Debug.LogError("Failed to get monster type to spawn");
            SetFailed();
            return;
        }
        Debug.Log($"Spawning {count} {monsterType}s");
    }
}

public class RewardBlockRTNode : BaseBlockRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<int>("Gold", graph, out var g))
        {
            Debug.LogError("Failed to get # gold to reward");
            SetFailed();
            return;
        }
        if (!TryGetInput<int>("XP", graph, out var xp))
        {
            Debug.LogError("Failed to get # xp to reward");
            SetFailed();
            return;
        }
        
        Debug.Log($"Awarding {g} gold and {xp} XP");
    }
}
