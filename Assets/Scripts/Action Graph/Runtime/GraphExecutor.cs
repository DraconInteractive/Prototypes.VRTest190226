using System;
using System.Collections;
using UnityEngine;

public class GraphExecutor : MonoBehaviour
{
    public ActionGraphAsset GraphAsset;

    public event Action<string> OnTrigger;

    private RuntimeActionGraph _activeGraph;

    private IEnumerator Start()
    {
        Debug.Log("Loading graph manager");
        yield return ActionGraphManager.EnsureLoaded();
        Debug.Log("Executing graph");
        Execute();
    }

    public void Execute()
    {
        if (GraphAsset == null)
        {
            Debug.LogError("GraphExecutor has no GraphAsset assigned.", this);
            return;
        }

        _activeGraph = GraphAsset.Provision();
        _activeGraph.CoroutineRunner = this;
        _activeGraph.OnTrigger = id => OnTrigger?.Invoke(id);
        _activeGraph.Execute();
    }

    [ContextMenu("Debug State")]
    public void StateCheckDebug()
    {
        if (_activeGraph == null)
        {
            Debug.LogError("No graph able to be debugged");
            return;
        }

        var debug = "";
        foreach (var node in _activeGraph.Nodes)
        {
            debug += $"[{node.GetType()}]: {node.State.ToString()}\n";
        }
        Debug.Log(debug);
    }
}
