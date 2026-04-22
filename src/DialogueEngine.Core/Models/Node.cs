namespace DialogueEngine.Core.Models;

/// <summary>
/// Nœud de dialogue.
/// Le moteur parcourt la liste de nœuds dans l'ordre et affiche
/// le premier dont la condition passe.
/// </summary>
public sealed record Node
{
    public required string        Id                   { get; init; }
    public          string?       ConditionKey         { get; init; }
    public required LocalizedText Text                 { get; init; }
    public          string?       ConsequenceKey       { get; init; }
    public          string?       CancelConsequenceKey { get; init; }
    public          Response[]    Responses            { get; init; } = [];
}
