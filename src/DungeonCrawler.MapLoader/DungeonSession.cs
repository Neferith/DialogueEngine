using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
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

    public int TurnNumber => _world?.TurnNumber ?? 0;

    public event Action<BiomeTextures?>? MapChanged;
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

        SubscribeToTurns(turns);

        var initialModule = _loader.GetModule(initialMap.ModuleId);
        CurrentBiomeTextures = initialModule != null
            ? ModuleTexturesConverter.Convert(initialModule)
            : null;

        if (_world != null)
            ApplyTileInventoryOverrides(initialMap, _world);
    }

    // ── API exposée au game loop ──────────────────────────────────────────────

    public DungeonView GetView() => Runner.GetView();
    public Party Party => Runner.Party;

    /// <summary>Appeler depuis PlayingScreen.OnEnter() pour déclencher les events MapEnter.</summary>
    public void NotifyMapEntered() => FireEvent(EventTrigger.MapEnter);

    public TurnResult ExecuteAction(PartyActionType action)
    {
        var result = Turns.ExecuteAction(action);

        if (result.PartyMoved)
        {
            CheckTransition();
            FireEvent(EventTrigger.TileEnter);
        }

        return result;
    }

    // ── Gestion des tours ─────────────────────────────────────────────────────

    private void SubscribeToTurns(TurnManager turns)
    {
        turns.TurnAdvanced += OnTurnAdvanced;
    }

    private void OnTurnAdvanced()
    {
        if (_world != null)
            _world.TurnNumber++;

        FireEvent(EventTrigger.TurnPassed);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void FireEvent(EventTrigger trigger)
    {
        if (_events == null || _world == null) return;

        var ctx = new EventContext
        {
            CurrentMapId = CurrentMap.Map.Name,
            TurnNumber = TurnNumber,
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
            Console.Error.WriteLine($"[DungeonSession] Map cible introuvable : {targetPath}");
            return;
        }

        var newMap = _loader.Load(targetPath, _modulesPath);

        var targetPos = new GridPosition(
            transition.TargetPosition.X,
            newMap.Map.Height - 1 - transition.TargetPosition.Y);

        Runner.Party.Teleport(targetPos, ParseDirection(transition.TargetOrientation));

        var newRunner = new DungeonRunner(newMap.Map, Runner.Party);
        var newTurns = new TurnManager(newRunner);

        // Désabonnement ancien TurnManager, abonnement au nouveau
        Turns.TurnAdvanced -= OnTurnAdvanced;
        SubscribeToTurns(newTurns);

        CurrentMap = newMap;
        Runner = newRunner;
        Turns = newTurns;

        if (_world != null)
            ApplyTileInventoryOverrides(newMap, _world);

        var newModule = _loader.GetModule(newMap.ModuleId);
        CurrentBiomeTextures = ModuleTexturesConverter.Convert(newModule ?? new());
        MapChanged?.Invoke(CurrentBiomeTextures);

        FireEvent(EventTrigger.MapEnter);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ApplyTileInventoryOverrides(LoadedMap map, WorldState world)
    {
        if (!world.TileInventoryOverrides.TryGetValue(map.Map.Name, out var overrides))
            return;

        foreach (var (key, items) in overrides)
        {
            var parts = key.Split('_');
            if (parts.Length != 2) continue;
            if (!int.TryParse(parts[0], out var x) ||
                !int.TryParse(parts[1], out var y)) continue;

            var tile = map.Map.GetTile(x, y);
            if (tile == null) continue;

            tile.FloorInventory.Clear();
            foreach (var (itemId, qty) in items)
                tile.FloorInventory.Add(itemId, qty);
        }
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