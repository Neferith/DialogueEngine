using CommunityToolkit.Mvvm.ComponentModel;
using MapEditor.Core.Models;
using MapEditor.Core.Modules;

namespace MapEditor.Avalonia.ViewModels;

public partial class MapGridViewModel : ObservableObject
{
    private readonly ModuleDefinition _module;

    // Sparse storage — only cells that differ from the default are kept
    private readonly Dictionary<(int, int), TileData>       _tileOverrides   = new();
    private readonly Dictionary<(int, int), EntityPlacement> _entityPlacements = new();

    public MapFile          MapFile { get; }
    public ModuleDefinition Module  => _module;

    [ObservableProperty] private int  _cellSize  = 32;
    [ObservableProperty] private int? _hoverX;
    [ObservableProperty] private int? _hoverY;
    [ObservableProperty] private int? _selectedX;
    [ObservableProperty] private int? _selectedY;

    /// <summary>Fired whenever the map data changes — causes the canvas to redraw.</summary>
    public event Action? MapChanged;

    public MapGridViewModel(MapFile mapFile, ModuleDefinition module)
    {
        MapFile = mapFile;
        _module = module;

        foreach (var tile   in mapFile.Tiles)    _tileOverrides  [(tile.Position.X,   tile.Position.Y)]   = tile;
        foreach (var entity in mapFile.Entities) _entityPlacements[(entity.Position.X, entity.Position.Y)] = entity;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public TileTypeDefinition DefaultTileType =>
        _module.FindTileType(MapFile.DefaultTileTypeId)
        ?? _module.TileTypes.FirstOrDefault()
        ?? new TileTypeDefinition { Id = "?", Name = "Unknown" };

    public TileTypeDefinition? GetTileTypeAt(int x, int y) =>
        _tileOverrides.TryGetValue((x, y), out var td)
            ? _module.FindTileType(td.TileTypeId)
            : DefaultTileType;

    public TileData? GetTileDataAt(int x, int y)
    {
        _tileOverrides.TryGetValue((x, y), out var td);
        return td;
    }

    public EntityPlacement? GetEntityAt(int x, int y)
    {
        _entityPlacements.TryGetValue((x, y), out var ep);
        return ep;
    }

    // ── Paint operations ──────────────────────────────────────────────────────

    public void PaintTile(int x, int y, TileTypeDefinition tileType)
    {
        if (!IsInBounds(x, y)) return;

        if (tileType.Id == MapFile.DefaultTileTypeId)
        {
            _tileOverrides.Remove((x, y));
        }
        else if (_tileOverrides.TryGetValue((x, y), out var existing))
        {
            _tileOverrides[(x, y)] = new TileData
            {
                Position   = existing.Position,
                TileTypeId = tileType.Id,
                Walkable   = tileType.DefaultWalkable,
                Transition = existing.Transition
            };
        }
        else
        {
            _tileOverrides[(x, y)] = new TileData
            {
                Position   = new PositionData(x, y),
                TileTypeId = tileType.Id,
                Walkable   = tileType.DefaultWalkable
            };
        }

        Sync(); MapChanged?.Invoke();
    }

    public void PlaceEntity(int x, int y, EntityTypeDefinition entityType)
    {
        if (!IsInBounds(x, y)) return;

        _entityPlacements[(x, y)] = new EntityPlacement
        {
            Id           = $"{entityType.Id}_{x}_{y}",
            EntityTypeId = entityType.Id,
            Position     = new PositionData(x, y),
            Orientation  = "NORTH",
            Properties   = entityType.Properties.ToDictionary(p => p.Key, p => p.Default)
        };

        Sync(); MapChanged?.Invoke();
    }

    public void EraseAt(int x, int y)
    {
        if (!IsInBounds(x, y)) return;
        _tileOverrides.Remove((x, y));
        _entityPlacements.Remove((x, y));
        Sync(); MapChanged?.Invoke();
    }

    /// <summary>Update tile data (walkable / transition) for an already-existing override.</summary>
    public void UpdateTileData(int x, int y, bool walkable, MapTransition? transition, List<TileItemData>? items = null)
    {
        if (!_tileOverrides.TryGetValue((x, y), out var existing)) return;
        _tileOverrides[(x, y)] = new TileData
        {
            Position   = existing.Position,
            TileTypeId = existing.TileTypeId,
            Walkable   = walkable,
            Transition = transition,
            Items = items ?? existing.Items
        };
        Sync(); MapChanged?.Invoke();
    }

    public void UpdateEntityPlacement(int x, int y, string orientation, Dictionary<string, string> properties)
    {
        if (!_entityPlacements.TryGetValue((x, y), out var existing)) return;
        _entityPlacements[(x, y)] = new EntityPlacement
        {
            Id           = existing.Id,
            EntityTypeId = existing.EntityTypeId,
            Position     = existing.Position,
            Orientation  = orientation,
            Properties   = properties
        };
        Sync();
    }

    // ── Hover / Selection ─────────────────────────────────────────────────────

    public void SetHover(int? x, int? y)
    {
        HoverX = x; HoverY = y;
        MapChanged?.Invoke();
    }

    public void SetSelection(int? x, int? y)
    {
        SelectedX = x; SelectedY = y;
        MapChanged?.Invoke();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void Sync()
    {
        MapFile.Tiles    = _tileOverrides.Values.ToList();
        MapFile.Entities = _entityPlacements.Values.ToList();
    }

    private bool IsInBounds(int x, int y) =>
        x >= 0 && x < MapFile.Size.Width && y >= 0 && y < MapFile.Size.Height;
}
