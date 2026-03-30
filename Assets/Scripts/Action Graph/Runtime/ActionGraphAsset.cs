using Newtonsoft.Json;
using UnityEngine;

public class ActionGraphAsset : ScriptableObject
{
    [TextArea(6, 20)]
    [JsonIgnore]
    public string SerializedGraph;

    public RuntimeActionGraph Provision() =>
        RuntimeActionGraph.Deserialize(SerializedGraph).Clone();
}
