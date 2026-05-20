using DungeonCrawler.EventSystems.Scripts;

namespace DungeonCrawler.EventSystems;

public class EventScriptRegistry
{
    private readonly Dictionary<string, IEventScript> _scripts = new();

    public EventScriptRegistry()
    {
        // Scripts built-in enregistrés par défaut
        Register(new SetFlagScript());
        Register(new ClearFlagScript());
        Register(new SetVariableScript());
        Register(new IncrVariableScript());
        Register(new StartDialogueScript());
        Register(new ShowMessageScript());
        Register(new GiveItemScript());
        Register(new NpcSetHostilityScript());
    }

    public EventScriptRegistry Register(IEventScript script)
    {
        _scripts[script.ScriptId] = script;
        return this;
    }

    public object? Execute(string scriptId, EventScriptContext ctx,
                            Dictionary<string, object> parameters)
    {
        if (_scripts.TryGetValue(scriptId, out var script))
            return script.Execute(ctx, parameters);

        Console.Error.WriteLine($"[EventScriptRegistry] Script inconnu : {scriptId}");
        return null;
    }

    /// <summary>Tous les scripts disponibles — utilisé par le toolset.</summary>
    public IReadOnlyCollection<IEventScript> All => _scripts.Values;
}