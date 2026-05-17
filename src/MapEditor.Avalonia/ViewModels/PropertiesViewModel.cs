using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Models;
using MapEditor.Core.Modules;
using MapEditor.Core.Serialization;
using System.Collections.ObjectModel;

namespace MapEditor.Avalonia.ViewModels;

public partial class PropertyEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _key = "";
    [ObservableProperty] private string _value = "";
}

public partial class PropertiesViewModel : ObservableObject
{
    // ── Dépendances injectées par EditorViewModel ─────────────────────────────
    private MapSerializer? _serializer;
    private Dictionary<string, string> _mapPaths = new(); // mapId → chemin absolu
    private MapGridViewModel? _grid;
    private int _px, _py;

    // ── Sélection ─────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private string _selectionLabel = "";

    // ── Section tile ──────────────────────────────────────────────────────────
    [ObservableProperty] private bool _showTileSection;
    [ObservableProperty] private string _tileTypeName = "";
    [ObservableProperty] private bool _tileWalkable;

    // ── Transition ────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _hasTransition;
    [ObservableProperty] private string? _selectedTargetMap;
    [ObservableProperty] private string _transitionArrivalX = "";
    [ObservableProperty] private string _transitionArrivalY = "";
    [ObservableProperty] private string _transitionArrivalOrientation = "SOUTH";
    [ObservableProperty] private string _transitionReturnOrientation = "NORTH";

    public ObservableCollection<string> AvailableMaps { get; } = new();

    // ── Section entité ────────────────────────────────────────────────────────
    [ObservableProperty] private bool _showEntitySection;
    [ObservableProperty] private string _entityTypeName = "";
    [ObservableProperty] private string _entityOrientation = "NORTH";

    public ObservableCollection<PropertyEntryViewModel> EntityProperties { get; } = new();
    public string[] Orientations { get; } = ["NORTH", "EAST", "SOUTH", "WEST"];

    // ── Initialisation (appelé par EditorViewModel au chargement du projet) ───

    public void Initialize(MapSerializer serializer, Dictionary<string, string> mapPaths)
    {
        _serializer = serializer;
        _mapPaths = mapPaths;

        AvailableMaps.Clear();
        foreach (var id in mapPaths.Keys.OrderBy(k => k))
            AvailableMaps.Add(id);
    }

    // ── API publique ──────────────────────────────────────────────────────────

    public void ShowTile(int x, int y, TileTypeDefinition tileType,
                         TileData? tileData, MapGridViewModel grid)
    {
        _grid = grid; _px = x; _py = y;

        HasSelection = true;
        SelectionLabel = $"Tile ({x}, {y})";
        ShowTileSection = true;
        ShowEntitySection = false;

        TileTypeName = tileType.Name;
        TileWalkable = tileData?.Walkable ?? tileType.DefaultWalkable;

        var t = tileData?.Transition;
        HasTransition = t != null;
        if (t != null)
        {
            SelectedTargetMap = t.TargetMapId;
            TransitionArrivalOrientation = t.TargetOrientation;
            TransitionArrivalX = t.TargetPosition.X.ToString();  
            TransitionArrivalY = t.TargetPosition.Y.ToString();  
        }
        else
        {
            SelectedTargetMap = AvailableMaps.FirstOrDefault();
            TransitionArrivalOrientation = "SOUTH";
            TransitionReturnOrientation = "NORTH";
            TransitionArrivalX = "";
            TransitionArrivalY = "";
        }
    }

    public void ShowEntity(int x, int y, EntityTypeDefinition entityType,
                           EntityPlacement entity, MapGridViewModel grid)
    {
        _grid = grid; _px = x; _py = y;

        HasSelection = true;
        SelectionLabel = $"Entité ({x}, {y})";
        ShowTileSection = false;
        ShowEntitySection = true;

        EntityTypeName = entityType.Name;
        EntityOrientation = entity.Orientation;

        EntityProperties.Clear();
        foreach (var kvp in entity.Properties)
            EntityProperties.Add(new PropertyEntryViewModel { Key = kvp.Key, Value = kvp.Value });
    }

    public void Clear()
    {
        HasSelection = false;
        ShowTileSection = false;
        ShowEntitySection = false;
        SelectionLabel = "";
        EntityProperties.Clear();
    }

    // ── Commandes ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ApplyTileProperties()
    {
        if (_grid == null) return;

        MapTransition? transition = null;

        if (HasTransition && !string.IsNullOrWhiteSpace(SelectedTargetMap))
        {
            int.TryParse(TransitionArrivalX, out var arrivalX);
            int.TryParse(TransitionArrivalY, out var arrivalY);
            var (adx, ady) = DirectionOffset(TransitionArrivalOrientation);

            // La porte est déduite de l'arrivée
            var doorX = arrivalX + adx;
            var doorY = arrivalY + ady;

            transition = new MapTransition
            {
                TargetMapId = SelectedTargetMap,
                TargetPosition = new PositionData(arrivalX, arrivalY),
                TargetOrientation = TransitionArrivalOrientation
            };
        }

        _grid.UpdateTileData(_px, _py, TileWalkable, transition);

        Console.WriteLine($"[Apply] HasTransition={HasTransition} transition={transition?.TargetMapId ?? "null"}");
    }

    [RelayCommand]
    private void CreateReturnTransition()
    {
        if (_grid == null || string.IsNullOrWhiteSpace(SelectedTargetMap)) return;

        int.TryParse(TransitionArrivalX, out var arrivalX);
        int.TryParse(TransitionArrivalY, out var arrivalY);
        var (adx, ady) = DirectionOffset(TransitionArrivalOrientation);

        var doorX = arrivalX - adx;
        var doorY = arrivalY - ady;

        var forwardTransition = new MapTransition
        {
            TargetMapId = SelectedTargetMap,
            TargetPosition = new PositionData(arrivalX, arrivalY),
            TargetOrientation = TransitionArrivalOrientation
        };

        ApplyReverseTransition(doorX, doorY, forwardTransition);
    }

    [RelayCommand]
    private void ApplyEntityProperties()
    {
        if (_grid == null) return;
        var props = EntityProperties.ToDictionary(e => e.Key, e => e.Value);
        _grid.UpdateEntityPlacement(_px, _py, EntityOrientation, props);
    }

    // ── Logique transition inverse ────────────────────────────────────────────

    private void ApplyReverseTransition(int targetDoorX, int targetDoorY,
                                         MapTransition forwardTransition)
    {
        if (_serializer == null) return;
        if (!_mapPaths.TryGetValue(forwardTransition.TargetMapId, out var targetPath)) return;

        var targetMap = _serializer.Load(targetPath);
        if (targetMap == null) return;

        // Spawn retour = porte source + offset(orientation retour)
        var (rdx, rdy) = DirectionOffset(TransitionReturnOrientation);
        var returnSpawn = new PositionData(_px + rdx, _py + rdy);

        var reverseTransition = new MapTransition
        {
            TargetMapId = _grid!.MapFile.Id,
            TargetPosition = returnSpawn,
            TargetOrientation = TransitionReturnOrientation
        };

        // Tile type de la porte source (pour créer une porte identique côté cible)
        var sourceTileTypeId = _grid.GetTileDataAt(_px, _py)?.TileTypeId ?? "STONE_DOOR";

        var existing = targetMap.Tiles.FirstOrDefault(
            t => t.Position.X == targetDoorX && t.Position.Y == targetDoorY);

        if (existing != null)
        {
            existing.TileTypeId = "STONE_DOOR";
            existing.Walkable = false;
            existing.Transition = reverseTransition;
        }
        else
        {
            targetMap.Tiles.Add(new TileData
            {
                Position = new PositionData(targetDoorX, targetDoorY),
                TileTypeId = "STONE_DOOR",
                Walkable = false,
                Transition = reverseTransition
            });
        }

        _serializer.Save(targetMap, targetPath);
        Console.WriteLine(
            $"[PropertiesViewModel] Transition inverse créée sur {forwardTransition.TargetMapId} " +
            $"en ({targetDoorX},{targetDoorY})");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (int dx, int dy) DirectionOffset(string orientation) =>
        orientation.ToUpperInvariant() switch
        {
            "NORTH" => (0, -1),
            "SOUTH" => (0, +1),
            "EAST" => (+1, 0),
            "WEST" => (-1, 0),
            _ => (0, 0)
        };
}