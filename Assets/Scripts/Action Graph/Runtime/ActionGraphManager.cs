using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ActionGraphManager
{
    public const string RegistryAddress = "ActionGraphRegistry";

    private static Dictionary<string, ActionGraphAsset> _graphs;
    private static AsyncOperationHandle<ActionGraphRegistry>? _handle;

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

    // Yields until the registry is loaded. Safe to call concurrently — all callers
    // share the same AsyncOperationHandle and only one load operation is ever issued.
    public static IEnumerator EnsureLoaded()
    {
        if (IsLoaded) yield break;

        if (!_handle.HasValue)
            _handle = Addressables.LoadAssetAsync<ActionGraphRegistry>(RegistryAddress);

        yield return _handle.Value;

        // Guard against multiple coroutines reaching this point simultaneously
        if (_graphs == null)
            _graphs = _handle.Value.Result.Graphs.ToDictionary(g => g.name);
    }
}
