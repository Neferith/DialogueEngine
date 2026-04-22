namespace DialogueEngine.Core.Models;

/// <summary>
/// Réponse proposée au joueur.
/// nextNodeIds : liste ordonnée de candidats — le moteur prend le premier
/// dont le nœud cible a une condition qui passe. Vide = fin du dialogue.
/// </summary>
public sealed record Response
{
    public required LocalizedText Text          { get; init; }
    public          string?       ConditionKey   { get; init; }
    public          string?       ConsequenceKey { get; init; }
    public          string[]      NextNodeIds    { get; set; } = [];
}
