using System.Text.Json;
using System.Text.Json.Serialization;
using DialogueEngine.Core.Models;

namespace DialogueEngine.Serialization;

/// <summary>
/// Gère le format union de LocalizedText :
///   "text": "string simple"
///   "text": [{ "conditionKey": "…", "value": "…" }, { "value": "…" }]
/// </summary>
public sealed class LocalizedTextConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String =>
                LocalizedText.Simple(reader.GetString()!),

            JsonTokenType.StartArray =>
                LocalizedText.FromDeserialization(
                    null,
                    JsonSerializer.Deserialize<TextVariant[]>(ref reader, options)
                        ?? throw new JsonException("Tableau de variantes vide.")),

            _ => throw new JsonException(
                $"LocalizedText : token inattendu '{reader.TokenType}'.")
        };

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options)
    {
        if (value.IsLocalized)
            JsonSerializer.Serialize(writer, value.Variants, options);
        else
            writer.WriteStringValue(value.SimpleText);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public static class DialogueFileSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented               = true,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        Converters                  = { new LocalizedTextConverter() }
    };

    public static DialogueEngine.Core.Models.DialogueFile Deserialize(string json)
        => JsonSerializer.Deserialize<DialogueEngine.Core.Models.DialogueFile>(json, Options)
           ?? throw new JsonException("Impossible de désérialiser le fichier.");

    public static string Serialize(DialogueEngine.Core.Models.DialogueFile file)
        => JsonSerializer.Serialize(file, Options);

    public static async Task<DialogueEngine.Core.Models.DialogueFile> DeserializeAsync(
        Stream stream, CancellationToken ct = default)
        => await JsonSerializer.DeserializeAsync<DialogueEngine.Core.Models.DialogueFile>(stream, Options, ct)
           ?? throw new JsonException("Impossible de désérialiser le fichier.");
}
