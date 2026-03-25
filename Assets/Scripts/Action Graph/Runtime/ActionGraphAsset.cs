using UnityEngine;

public class ActionGraphAsset : ScriptableObject
{
    [TextArea(6, 20)]
    public string SerializedGraph;

    public RuntimeActionGraph Provision() =>
        RuntimeActionGraph.Deserialize(SerializedGraph).Clone();
}
