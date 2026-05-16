using DungeonCrawler.Core.Models;
using MapEditor.Core.Models;
using MapEditor.Core.Modules;
using MapEditor.Core.Serialization;

namespace DungeonCrawler.MapLoader;

/// <summary>
/// Charge un MapFile (JSON éditeur) et le convertit en LoadedMap (monde moteur).
/// Usage :
///   var loader = new MapFileLoader();
///   var loaded = loader.Load("maps/dungeon_01.map.json", "modules/");
/// </summary>
public class MapFileLoader
{
    private readonly MapSerializer _serializer;
    private readonly ModuleLoader  _moduleLoader;

    // Cache des modules déjà chargés (un dossier modules/ peut contenir plusieurs modules)
    private readonly Dictionary<string, ModuleDefinition> _moduleCache = new();
    private string? _lastModulesPath;

    public MapFileLoader(MapSerializer? serializer = null, ModuleLoader? moduleLoader = null)
    {
        _serializer   = serializer   ?? new MapSerializer();
        _moduleLoader = moduleLoader ?? new ModuleLoader();
    }

    // ── Chargement depuis fichier ─────────────────────────────────────────────

    /// <summary>
    /// Charge une map depuis son chemin JSON et le dossier modules/.
    /// </summary>
    public LoadedMap Load(string mapFilePath, string modulesPath)
    {
        var mapFile = _serializer.Load(mapFilePath)
            ?? throw new InvalidOperationException($"Impossible de lire : {mapFilePath}");

        return Convert(mapFile, modulesPath);
    }

    /// <summary>
    /// Charge une map depuis un MapFile déjà désérialisé.
    /// </summary>
    public LoadedMap Convert(MapFile mapFile, string modulesPath)
    {
        EnsureModulesLoaded(modulesPath);

        if (!_moduleCache.TryGetValue(mapFile.ModuleId, out var module))
            throw new InvalidOperationException(
                $"Module '{mapFile.ModuleId}' introuvable dans '{modulesPath}'. " +
                $"Modules disponibles : {string.Join(", ", _moduleCache.Keys)}");

        return BuildLoadedMap(mapFile, module);
    }

    // ── Conversion ────────────────────────────────────────────────────────────

    private LoadedMap BuildLoadedMap(MapFile mapFile, ModuleDefinition module)
    {
        var width = mapFile.Size.Width;
        var height = mapFile.Size.Height;

        var dungeonMap = new DungeonMap(width, height, mapFile.Id);

        // Flip Y : éditeur = Y↓ (écran), moteur = Y↑ (math)
        int FlipY(int y) => height - 1 - y;

        var defaultDef = module.FindTileType(mapFile.DefaultTileTypeId);
        if (defaultDef != null)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    dungeonMap.SetTile(x, y, TileConverter.Convert(defaultDef));
        }

        var transitions = new Dictionary<GridPosition, MapTransition>();

        foreach (var tileData in mapFile.Tiles)
        {
            var def = module.FindTileType(tileData.TileTypeId);
            if (def == null)
            {
                Console.Error.WriteLine($"[MapFileLoader] TileType inconnu '{tileData.TileTypeId}' — ignoré.");
                continue;
            }

            var tile = TileConverter.Convert(def, walkableOverride: tileData.Walkable);
            dungeonMap.SetTile(tileData.Position.X, FlipY(tileData.Position.Y), tile);

            if (tileData.Transition != null)
                transitions[new GridPosition(tileData.Position.X, FlipY(tileData.Position.Y))] = tileData.Transition;
        }

        GridPosition? playerSpawn = null;
        Direction playerFacing = Direction.North;

        foreach (var entity in mapFile.Entities)
        {
            if (!string.Equals(entity.EntityTypeId, "PLAYER_SPAWN",
                               StringComparison.OrdinalIgnoreCase)) continue;

            playerSpawn = new GridPosition(entity.Position.X, FlipY(entity.Position.Y));
            playerFacing = ParseDirection(entity.Orientation);
            break;
        }

        return new LoadedMap(dungeonMap, transitions, mapFile.Entities,
                             playerSpawn, playerFacing, mapFile.ModuleId);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void EnsureModulesLoaded(string modulesPath)
    {
        if (_lastModulesPath == modulesPath && _moduleCache.Count > 0) return;

        _moduleCache.Clear();
        _lastModulesPath = modulesPath;

        foreach (var module in _moduleLoader.LoadAll(modulesPath))
            _moduleCache[module.Id] = module;
    }

    private static Direction ParseDirection(string orientation) =>
        orientation.ToUpperInvariant() switch
        {
            "NORTH" => Direction.North,
            "EAST"  => Direction.East,
            "SOUTH" => Direction.South,
            "WEST"  => Direction.West,
            _       => Direction.North
        };
}
