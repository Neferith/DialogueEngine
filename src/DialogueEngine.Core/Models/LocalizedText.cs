namespace DialogueEngine.Core.Models;

/// <summary>
/// Variante de texte conditionnelle.
/// La dernière entrée sans ConditionKey est le fallback obligatoire.
/// </summary>
public sealed record TextVariant
{
    public string? ConditionKey { get; init; }
    public required string Value { get; init; }
}

/// <summary>
/// Texte localisable : string simple ou liste de variantes conditionnelles.
/// Sérialisé en JSON comme string ou tableau selon le cas.
/// </summary>
public sealed class LocalizedText
{
    private readonly string?        _simple;
    private readonly TextVariant[]? _variants;

    private LocalizedText(string simple)          => _simple   = simple;
    private LocalizedText(TextVariant[] variants) => _variants = variants;

    public static LocalizedText Simple(string text)              => new(text);
    public static LocalizedText Localized(TextVariant[] variants) => new(variants);

    public bool                         IsLocalized => _variants is not null;
    public string?                      SimpleText  => _simple;
    public IReadOnlyList<TextVariant>?  Variants    => _variants;

    public static implicit operator LocalizedText(string text) => Simple(text);

    // Réservé à la désérialisation
    public static LocalizedText FromDeserialization(string? simple, TextVariant[]? variants)
    {
        if (variants is not null) return new(variants);
        if (simple   is not null) return new(simple);
        throw new InvalidOperationException("LocalizedText : simple ou variants requis.");
    }
}
