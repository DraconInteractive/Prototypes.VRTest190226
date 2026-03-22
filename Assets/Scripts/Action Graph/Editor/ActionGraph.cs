using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

[Serializable]
[Graph(AssetExtension)]
internal class ActionGraph : Graph
{
    internal const string AssetExtension = "egraph";

    [MenuItem("Assets/Create/Action Graph", false)]
    static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<ActionGraph>();
    }
}