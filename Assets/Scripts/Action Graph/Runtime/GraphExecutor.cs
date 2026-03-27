using System.Collections;
using UnityEngine;

public class GraphExecutor : MonoBehaviour
{
    public ActionGraphAsset GraphAsset;

    private IEnumerator Start()
    {
        yield return ActionGraphManager.EnsureLoaded();
        Execute();
    }

    public void Execute()
    {
        if (GraphAsset == null)
        {
            Debug.LogError("GraphExecutor has no GraphAsset assigned.", this);
            return;
        }

        GraphAsset.Provision().Execute();
    }
}
