using UnityEngine;

public class GraphExecutor : MonoBehaviour
{
    public ActionGraphAsset GraphAsset;

    private void Start()
    {
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
