public class SpawnWaveRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        // TODO: read block data (monster types, counts, spawn points) and trigger wave spawning
        DefExecNext(graph);
    }
}
