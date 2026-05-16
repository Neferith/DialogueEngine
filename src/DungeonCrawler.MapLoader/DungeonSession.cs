using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.MapLoader;

/// <summary>
/// Gère la session de jeu courante : map active, runner, et transitions entre maps.
/// C'est le point d'entrée unique pour le game loop — remplace l'accès direct à
/// DungeonRunner + TurnManager.
/// </summary>
public class DungeonSession
{
    private readonly MapFileLoader _loader;
    private readonly string _mapsPath;
    private readonly string _modulesPath;

    public LoadedMap CurrentMap { get; private set; }
    public DungeonRunner Runner { get; private set; }
    public TurnManager Turns { get; private set; }

    /// <summary>Déclenché quand une transition a changé la map courante.</summary>
    public event Action? MapChanged;

    public DungeonSession(
        LoadedMap initialMap,
        DungeonRunner runner,
        TurnManager turns,
        MapFileLoader loader,
        string mapsPath,
        string modulesPath)
    {
        CurrentMap = initialMap;
        Runner = runner;
        Turns = turns;
        _loader = loader;
        _mapsPath = mapsPath;
        _modulesPath = modulesPath;
    }

    // ── API exposée au game loop ──────────────────────────────────────────────

    public DungeonView GetView() => Runner.GetView();
    public Party Party => Runner.Party;
    public int TurnNumber => Turns.TurnNumber;

    public TurnResult ExecuteAction(PartyActionType action)
    {
        var result = Turns.ExecuteAction(action);

        if (result.PartyMoved)
            CheckTransition();

        return result;
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

        // La position cible est en coordonnées éditeur → flip Y
        var targetPos = new GridPosition(
            transition.TargetPosition.X,
            newMap.Map.Height - 1 - transition.TargetPosition.Y);

        var targetFacing = ParseDirection(transition.TargetOrientation);

        // On conserve la party (membres, stats) — on change juste position + orientation
        Runner.Party.Teleport(targetPos, targetFacing);

        // Swap du runner et du turn manager sur la nouvelle map
        var newEntities = new EntitySystem();
        var newRunner = new DungeonRunner(newMap.Map, Runner.Party, newEntities);
        var newTurns = new TurnManager(newRunner, newEntities);

        CurrentMap = newMap;
        Runner = newRunner;
        Turns = newTurns;

        MapChanged?.Invoke();
        Console.WriteLine($"[DungeonSession] Transition → {transition.TargetMapId}");
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