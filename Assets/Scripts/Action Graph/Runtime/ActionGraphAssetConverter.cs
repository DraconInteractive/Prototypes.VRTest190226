using System;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Serializes ActionGraphAsset as just its asset name.
/// Deserialization resolves the name via ActionGraphManager (requires EnsureLoaded to have completed).
/// </summary>
public class ActionGraphAssetConverter : JsonConverter<ActionGraphAsset>
{
    public override void WriteJson(JsonWriter writer, ActionGraphAsset value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.name);
    }

    public override ActionGraphAsset ReadJson(JsonReader reader, Type objectType, ActionGraphAsset existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var name = reader.Value?.ToString();
        return string.IsNullOrEmpty(name) ? null : ActionGraphManager.Get(name);
    }
}
