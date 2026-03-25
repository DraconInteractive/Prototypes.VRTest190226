using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionGraphAsset))]
public class ActionGraphAssetInspector : Editor
{
    private bool _jsonFoldout;

    public override void OnInspectorGUI()
    {
        var asset = (ActionGraphAsset)target;

        if (string.IsNullOrEmpty(asset.SerializedGraph))
        {
            EditorGUILayout.HelpBox("SerializedGraph is empty — not imported correctly.", MessageType.Error);
            return;
        }

        var graph = RuntimeActionGraph.Deserialize(asset.SerializedGraph);
        EditorGUILayout.LabelField("Nodes", graph.Nodes?.Count.ToString() ?? "null");

        _jsonFoldout = EditorGUILayout.Foldout(_jsonFoldout, "Serialized JSON", toggleOnLabelClick: true);
        if (_jsonFoldout)
        {
            EditorGUILayout.TextArea(asset.SerializedGraph);
        }
    }
}
