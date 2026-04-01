using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActionGraphManager
{
    private static Dictionary<string, ActionGraphAsset> _graphs;

    public static bool IsLoaded => _graphs != null;

    public static ActionGraphAsset Get(string name)
    {
        if (_graphs == null)
        {
            Debug.LogError("ActionGraphManager: registry not loaded. Ensure GraphExecutor has initialised the manager before executing.");
            return null;
        }

        if (!_graphs.TryGetValue(name, out var asset))
            Debug.LogError($"ActionGraphManager: no graph found with name '{name}'.");

        return asset;
    }

    public static IEnumerator EnsureLoaded()
    {
        if (IsLoaded) yield break;

        var registry = Resources.Load<ActionGraphRegistry>("ActionGraphRegistry");
        if (registry == null)
        {
            Debug.LogError("ActionGraphManager: ActionGraphRegistry not found in Resources.");
            yield break;
        }

        _graphs = registry.Graphs.ToDictionary(g => g.name);
    }
}
