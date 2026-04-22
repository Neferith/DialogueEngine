namespace DialogueEngine.Core.Models;

/// <summary>
/// Fichier de dialogue. Non lié à un NPC spécifique —
/// le même dialogue peut être assigné à plusieurs NPC.
/// </summary>
public sealed record DialogueFile
{
    public required string   Id    { get; init; }
    public          Node[]   Nodes { get; init; } = [];
}
