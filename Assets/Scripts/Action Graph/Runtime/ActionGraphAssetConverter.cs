using System;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Serializes ActionGraphAsset as a lightweight asset path reference rather than
/// embedding the full object data. Deserialization loads the asset by path.
/// </summary>
public class ActionGraphAssetConverter : JsonConverter<ActionGraphAsset>
{
    public override void WriteJson(JsonWriter writer, ActionGraphAsset value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

#if UNITY_EDITOR
        var path = UnityEditor.AssetDatabase.GetAssetPath(value);
        writer.WriteValue(path);
#else
        Debug.LogError("ActionGraphAssetConverter: cannot serialize ActionGraphAsset outside of the editor.");
        writer.WriteNull();
#endif
    }

    public override ActionGraphAsset ReadJson(JsonReader reader, Type objectType, ActionGraphAsset existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var path = reader.Value?.ToString();
        if (string.IsNullOrEmpty(path))
            return null;

#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<ActionGraphAsset>(path);
#else
        Debug.LogError($"ActionGraphAssetConverter: cannot load ActionGraphAsset at '{path}' outside of the editor. Runtime loading via Addressables or Resources is not yet implemented.");
        return null;
#endif
    }
}
