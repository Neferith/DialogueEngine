using DungeonCrawler.Persistence;

namespace DungeonCrawler.EventSystems;

/// <summary>
/// Exécute les conséquences custom enregistrées par clé.
/// Pattern identique au ScriptRegistry du DialogueEngine.
/// </summary>
public class EventRegistry
{
    private readonly Dictionary<string, Action<WorldState>> _consequences = new();

    public EventRegistry Consequence(string key, Action<WorldState> action)
    {
        _consequences[key] = action;
        return this;
    }

    public void Execute(string key, WorldState world)
    {
        if (_consequences.TryGetValue(key, out var action))
            action(world);
        else
            Console.Error.WriteLine($"[EventRegistry] Clé inconnue : {key}");
    }
}