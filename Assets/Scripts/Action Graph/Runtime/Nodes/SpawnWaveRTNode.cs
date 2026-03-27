using System.Collections.Generic;

public class SpawnWaveRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        // TODO: read block data (monster types, counts, spawn points) and trigger wave spawning
        DefExecNext(graph);
    }
}

public class SpawnMonsterRTBlockNode : BaseRTNode
{
    public enum MonsterType
    {
        Wolf,
        Demon,
        Angel
    }

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        
    }
}

public class RewardBlockRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
    }
}
