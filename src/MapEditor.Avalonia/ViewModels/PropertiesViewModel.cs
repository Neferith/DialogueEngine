using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Models;
using MapEditor.Core.Modules;
using System.Collections.ObjectModel;

namespace MapEditor.Avalonia.ViewModels;

public partial class PropertyEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _key   = "";
    [ObservableProperty] private string _value = "";
}

public partial class PropertiesViewModel : ObservableObject
{
    private MapGridViewModel?    _grid;
    private int                  _px, _py;

    // ── Selection state ───────────────────────────────────────────────────────

    [ObservableProperty] private bool   _hasSelection;
    [ObservableProperty] private string _selectionLabel = "";

    // ── Tile section ──────────────────────────────────────────────────────────

    [ObservableProperty] private bool   _showTileSection;
    [ObservableProperty] private string _tileTypeName   = "";
    [ObservableProperty] private bool   _tileWalkable;
    [ObservableProperty] private bool   _hasTransition;
    [ObservableProperty] private string _transitionTargetMapId  = "";
    [ObservableProperty] private string _transitionTargetX      = "";
    [ObservableProperty] private string _transitionTargetY      = "";
    [ObservableProperty] private string _transitionOrientation  = "NORTH";

    // ── Entity section ────────────────────────────────────────────────────────

    [ObservableProperty] private bool   _showEntitySection;
    [ObservableProperty] private string _entityTypeName   = "";
    [ObservableProperty] private string _entityOrientation = "NORTH";

    public ObservableCollection<PropertyEntryViewModel> EntityProperties { get; } = new();

    public string[] Orientations { get; } = ["NORTH", "EAST", "SOUTH", "WEST"];

    // ── Public API ────────────────────────────────────────────────────────────

    public void ShowTile(int x, int y, TileTypeDefinition tileType,
                         TileData? tileData, MapGridViewModel grid)
    {
        _grid = grid; _px = x; _py = y;

        HasSelection    = true;
        SelectionLabel  = $"Tile at ({x}, {y})";
        ShowTileSection   = true;
        ShowEntitySection = false;

        TileTypeName = tileType.Name;
        TileWalkable = tileData?.Walkable ?? tileType.DefaultWalkable;

        var t = tileData?.Transition;
        HasTransition           = t != null;
        TransitionTargetMapId   = t?.TargetMapId       ?? "";
        TransitionTargetX       = t?.TargetPosition.X.ToString() ?? "";
        TransitionTargetY       = t?.TargetPosition.Y.ToString() ?? "";
        TransitionOrientation   = t?.TargetOrientation ?? "NORTH";
    }

    public void ShowEntity(int x, int y, EntityTypeDefinition entityType,
                           EntityPlacement entity, MapGridViewModel grid)
    {
        _grid = grid; _px = x; _py = y;

        HasSelection      = true;
        SelectionLabel    = $"Entity at ({x}, {y})";
        ShowTileSection   = false;
        ShowEntitySection = true;

        EntityTypeName    = entityType.Name;
        EntityOrientation = entity.Orientation;

        EntityProperties.Clear();
        foreach (var kvp in entity.Properties)
            EntityProperties.Add(new PropertyEntryViewModel { Key = kvp.Key, Value = kvp.Value });
    }

    public void Clear()
    {
        HasSelection      = false;
        ShowTileSection   = false;
        ShowEntitySection = false;
        SelectionLabel    = "";
        EntityProperties.Clear();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ApplyTileProperties()
    {
        if (_grid == null) return;

        MapTransition? transition = null;
        if (HasTransition && !string.IsNullOrWhiteSpace(TransitionTargetMapId))
        {
            int.TryParse(TransitionTargetX, out var tx);
            int.TryParse(TransitionTargetY, out var ty);
            transition = new MapTransition
            {
                TargetMapId        = TransitionTargetMapId,
                TargetPosition     = new PositionData(tx, ty),
                TargetOrientation  = TransitionOrientation
            };
        }

        _grid.UpdateTileData(_px, _py, TileWalkable, transition);
    }

    [RelayCommand]
    private void ApplyEntityProperties()
    {
        if (_grid == null) return;
        var props = EntityProperties.ToDictionary(e => e.Key, e => e.Value);
        _grid.UpdateEntityPlacement(_px, _py, EntityOrientation, props);
    }
}
