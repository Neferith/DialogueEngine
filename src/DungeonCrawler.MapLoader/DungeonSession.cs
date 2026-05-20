using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.EventSystems;
using DungeonCrawler.Persistence;

namespace DungeonCrawler.MapLoader;

public class DungeonSession
{
    private readonly MapFileLoader _loader;
    private readonly string _mapsPath;
    private readonly string _modulesPath;
    private readonly EventSystem? _events;
    private readonly WorldState? _world;

    public LoadedMap CurrentMap { get; private set; }
    public DungeonRunner Runner { get; private set; }
    public TurnManager Turns { get; private set; }
    public BiomeTextures? CurrentBiomeTextures { get; private set; }

    public event Action<BiomeTextures?>? MapChanged;

    /// <summary>Relayé depuis l'EventSystem interne.</summary>
    public event Action<GameEvent, IReadOnlyList<IGameAction>>? EventFired;

    public DungeonSession(
        LoadedMap initialMap,
        DungeonRunner runner,
        TurnManager turns,
        MapFileLoader loader,
        string mapsPath,
        string modulesPath,
        EventSystem? events = null,
        WorldState? world = null)
    {
        CurrentMap = initialMap;
        Runner = runner;
        Turns = turns;
        _loader = loader;
        _mapsPath = mapsPath;
        _modulesPath = modulesPath;
        _events = events;
        _world = world;

        if (events != null)
            events.EventFired += (ev, actions) => EventFired?.Invoke(ev, actions);

        var initialModule = _loader.GetModule(initialMap.ModuleId);
        CurrentBiomeTextures = initialModule != null
            ? ModuleTexturesConverter.Convert(initialModule)
            : null;
    }

    // ── API exposée au game loop ──────────────────────────────────────────────

    public DungeonView GetView() => Runner.GetView();
    public Party Party => Runner.Party;
    public int TurnNumber => Turns.TurnNumber;

    /// <summary>Appeler après OnEnter pour déclencher les events MapEnter initiaux.</summary>
    public void NotifyMapEntered()
    {
        FireEvent(EventTrigger.MapEnter);
    }

    public TurnResult ExecuteAction(PartyActionType action)
    {
        var result = Turns.ExecuteAction(action);

        if (result.PartyMoved)
        {
            CheckTransition();
            FireEvent(EventTrigger.TileEnter);
        }

        FireEvent(EventTrigger.TurnPassed);

        return result;
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void FireEvent(EventTrigger trigger)
    {
        if (_events == null || _world == null) return;

        var ctx = new EventContext
        {
            CurrentMapId = CurrentMap.Map.Name,
            TurnNumber = Turns.TurnNumber,
            PlayerPos = new GridPos(Runner.Party.Position.X, Runner.Party.Position.Y)
        };

        _events.Trigger(trigger, _world, ctx);
    }

    // ── Transitions ───────────────────────────────────────────────────────────

    private void CheckTransition()
    {
        var transition = CurrentMap.GetTransitionAt(Runner.Party.Position);
        if (transition == null) return;

        var targetPath = Path.Combine(_mapsPath, $"{transition.TargetMapId}.map.json");
        if (!File.Exists(targetPath))
        {
            Console.Error.WriteLine(
                $"[DungeonSession] Map cible introuvable : {targetPath}");
            return;
        }

        var newMap = _loader.Load(targetPath, _modulesPath);

        var targetPos = new GridPosition(
            transition.TargetPosition.X,
            newMap.Map.Height - 1 - transition.TargetPosition.Y);

        Runner.Party.Teleport(targetPos, ParseDirection(transition.TargetOrientation));

        var newEntities = new EntitySystem();
        var newRunner = new DungeonRunner(newMap.Map, Runner.Party, newEntities);
        var newTurns = new TurnManager(newRunner, newEntities);

        CurrentMap = newMap;
        Runner = newRunner;
        Turns = newTurns;

        var newModule = _loader.GetModule(newMap.ModuleId);
        CurrentBiomeTextures = ModuleTexturesConverter.Convert(newModule ?? new());
        MapChanged?.Invoke(CurrentBiomeTextures);

        // Déclencher MapEnter sur la nouvelle map
        FireEvent(EventTrigger.MapEnter);
    }

    private static Direction ParseDirection(string orientation) =>
        orientation.ToUpperInvariant() switch
        {
            "NORTH" => Direction.North,
            "EAST" => Direction.East,
            "SOUTH" => Direction.South,
            "WEST" => Direction.West,
            _ => Direction.North
        };
}