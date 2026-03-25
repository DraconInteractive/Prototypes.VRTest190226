using UnityEngine;

public class RunGraphRTNode : BaseRTNode
{
    // Populated by the importer — resolved from the "Graph" ActionGraph port reference
    // to its corresponding ActionGraphAsset. See ActionGraphImporter for TODO.
    public ActionGraphAsset SubGraph;

    protected override void ExecuteInternal()
    {
        if (!TryGetInput<ActionGraphAsset>("Graph", out var graphAsset))
        {
            SetFailed();
            return;
        }

        if (!TryGetInput<bool>("Wait?", out bool wait))
        {
            SetFailed();
            return;
        }

        var graph = graphAsset.Provision();
        graph.Execute();

        // TODO: when async node support is added, respect the Wait? flag —
        // if false, DefExecNext() should fire immediately rather than waiting
        // for the sub-graph to complete.
        DefExecNext();
    }
}
