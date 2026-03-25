using UnityEngine;

public class ActionGraphAsset : ScriptableObject
{
    [SerializeReference]
    public RuntimeActionGraph Graph;

    public RuntimeActionGraph Provision() => Graph.Clone();
}
