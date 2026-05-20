using DungeonCrawler.EventSystems;
using MapEditor.Core.Events;

namespace DungeonCrawler.EventSystems;

public class EventLoader
{
    // ── Chargement depuis le dossier events/ ──────────────────────────────────

    public void LoadInto(EventSystem eventSystem, string eventsPath,
                          EventScriptRegistry registry)
    {
        if (!Directory.Exists(eventsPath))
        {
            Console.Error.WriteLine($"[EventLoader] Dossier introuvable : {eventsPath}");
            return;
        }

        // global/
        LoadFolder(eventSystem, registry,
            Path.Combine(eventsPath, "global"));

        // maps/
        LoadFolder(eventSystem, registry,
            Path.Combine(eventsPath, "maps"));

        // Futurs dossiers : npcs/, items/, quests/…
        // LoadFolder(eventSystem, registry, Path.Combine(eventsPath, "npcs"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void LoadFolder(EventSystem eventSystem,
                                    EventScriptRegistry registry,
                                    string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        foreach (var file in Directory.GetFiles(folderPath, "*.events.json"))
        {
            var eventFile = EventSerializer.Load(file);
            foreach (var data in eventFile.Events)
            {
                var gameEvent = Convert(data, registry);
                if (gameEvent != null)
                    eventSystem.Register(gameEvent);
            }
            Console.WriteLine($"[EventLoader] {Path.GetFileName(file)} — " +
                              $"{eventFile.Events.Count} event(s)");
        }
    }

    private static GameEvent? Convert(EventData data, EventScriptRegistry registry)
    {
        if (!Enum.TryParse<EventTrigger>(data.Trigger, ignoreCase: true, out var trigger))
        {
            Console.Error.WriteLine($"[EventLoader] Trigger inconnu : {data.Trigger}");
            return null;
        }

        return new GameEvent
        {
            Id = data.Id,
            Trigger = trigger,
            MapId = data.MapId,
            TileX = data.TileX,
            TileY = data.TileY,
            EntityId = data.EntityId,
            Radius = data.Radius,
            Condition = new EventCondition
            {
                FlagNotSet = data.Condition.FlagNotSet,
                FlagSet = data.Condition.FlagSet,
                NpcAlive = data.Condition.NpcAlive,
                NpcNotAlive = data.Condition.NpcNotAlive,
                MinTurn = data.Condition.MinTurn
            },
            Effects = data.Effects
                .Select(e => ConvertEffect(e, registry))
                .ToList()
        };
    }

    private static EventEffect ConvertEffect(EventEffectData data,
                                              EventScriptRegistry registry)
    {
        // Convertit les params string → object selon le type déclaré dans le script
        var script = registry.All.FirstOrDefault(s => s.ScriptId == data.ScriptId);
        var converted = new Dictionary<string, object>();

        foreach (var (key, strValue) in data.Params)
        {
            var paramDef = script?.Parameters.FirstOrDefault(p => p.Name == key);
            converted[key] = paramDef?.Type switch
            {
                "int" => int.TryParse(strValue, out var i) ? (object)i : 0,
                "bool" => bool.TryParse(strValue, out var b) ? (object)b : false,
                _ => strValue  // string par défaut
            };
        }

        return new EventEffect { ScriptId = data.ScriptId, Params = converted };
    }
}