using DialogueEngine.Core.Interfaces;

namespace DialogueEngine.Core.Engine;

/// <summary>
/// Registre des scripts nommés. Le jeu y enregistre ses implémentations,
/// le moteur les résout par clé.
/// </summary>
public sealed class ScriptRegistry
{
    private readonly Dictionary<string, IConditionScript>   _conditions   = [];
    private readonly Dictionary<string, IConsequenceScript> _consequences = [];

    public ScriptRegistry Condition(string key, IConditionScript script)
    {
        _conditions[key] = script;
        return this;
    }

    public ScriptRegistry Condition(string key, Func<IDialogueContext, bool> fn)
        => Condition(key, new LambdaCondition(fn));

    public ScriptRegistry Consequence(string key, IConsequenceScript script)
    {
        _consequences[key] = script;
        return this;
    }

    public ScriptRegistry Consequence(string key, Action<IDialogueContext> fn)
        => Consequence(key, new LambdaConsequence(fn));

    internal bool Evaluate(string key, IDialogueContext ctx)
    {
        if (!_conditions.TryGetValue(key, out var script))
            throw new KeyNotFoundException($"Condition inconnue : '{key}'");
        return script.Evaluate(ctx);
    }

    internal void Execute(string key, IDialogueContext ctx)
    {
        if (!_consequences.TryGetValue(key, out var script))
            throw new KeyNotFoundException($"Conséquence inconnue : '{key}'");
        script.Execute(ctx);
    }

    // ── Wrappers lambda ───────────────────────────────────────────────────

    private sealed class LambdaCondition(Func<IDialogueContext, bool> fn) : IConditionScript
    {
        public bool Evaluate(IDialogueContext ctx) => fn(ctx);
    }

    private sealed class LambdaConsequence(Action<IDialogueContext> fn) : IConsequenceScript
    {
        public void Execute(IDialogueContext ctx) => fn(ctx);
    }
}
