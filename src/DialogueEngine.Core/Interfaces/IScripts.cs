namespace DialogueEngine.Core.Interfaces;

/// <summary>
/// Script de condition. Reçoit le contexte, retourne un bool.
/// Enregistré dans le DialogueContext par le jeu.
/// </summary>
public interface IConditionScript
{
    bool Evaluate(IDialogueContext context);
}

/// <summary>
/// Script de conséquence. Exécute un effet de bord.
/// </summary>
public interface IConsequenceScript
{
    void Execute(IDialogueContext context);
}

/// <summary>
/// Résout une variable nommée en string pour substitution dans les textes.
/// Retourne la clé entre accolades si inconnue — jamais d'exception.
/// </summary>
public interface IVariableResolver
{
    string Resolve(string key);
}

/// <summary>
/// Contexte injecté dans tous les scripts.
/// Exposé au jeu via cette interface — le moteur ne sait pas ce qu'il y a dedans.
/// </summary>
public interface IDialogueContext
{
    IVariableResolver Variables { get; }
}
