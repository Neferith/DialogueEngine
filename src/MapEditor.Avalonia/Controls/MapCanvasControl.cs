using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MapEditor.Avalonia.ViewModels;
using MapEditor.Core.Modules;

namespace MapEditor.Avalonia.Controls;

/// <summary>
/// Custom Avalonia control that renders the dungeon map grid.
/// DataContext must be the root <see cref="EditorViewModel"/>.
/// </summary>
public class MapCanvasControl : Control
{
    // ── Brushes / pens (allocated once) ──────────────────────────────────────

    private static readonly IPen   GridPen       = new Pen(Brushes.Black, 1);
    private static readonly IBrush HoverBrush    = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));
    private static readonly IPen   SelectionPen  = new Pen(new SolidColorBrush(Colors.Yellow), 2);

    private static readonly Typeface EntityFont = new("default");

    // Fallback tile colors
    private static readonly IBrush FallbackWall  = new SolidColorBrush(Color.Parse("#3A3A4A"));
    private static readonly IBrush FallbackFloor = new SolidColorBrush(Color.Parse("#7A7A8A"));
    private static readonly IBrush FallbackDoor  = new SolidColorBrush(Color.Parse("#8B5E3C"));

    // Entity fallback colors by category
    private static readonly IBrush SpawnBrush = new SolidColorBrush(Color.Parse("#00C853"));
    private static readonly IBrush NpcBrush   = new SolidColorBrush(Color.Parse("#448AFF"));
    private static readonly IBrush ItemBrush  = new SolidColorBrush(Color.Parse("#FFD600"));
    private static readonly IBrush UnknownBrush = new SolidColorBrush(Color.Parse("#FF5252"));

    // ── Brush cache (per loaded module) ──────────────────────────────────────

    private readonly Dictionary<string, IBrush> _tileBrushCache   = new();
    private readonly Dictionary<string, IBrush> _entityBrushCache = new();
    private EditorViewModel? _vm;
    private bool _isPressed;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Detach from old VM
        if (_vm?.MapGrid != null)
            _vm.MapGrid.MapChanged -= OnMapChanged;
        if (_vm != null)
            _vm.PropertyChanged -= OnEditorVmPropertyChanged;

        _vm = DataContext as EditorViewModel;
        _tileBrushCache.Clear();
        _entityBrushCache.Clear();

        if (_vm != null)
        {
            _vm.PropertyChanged += OnEditorVmPropertyChanged;
            if (_vm.MapGrid != null) _vm.MapGrid.MapChanged += OnMapChanged;
        }

        InvalidateMeasure();
        InvalidateVisual();
    }

    private void OnEditorVmPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EditorViewModel.MapGrid)) return;

        // Re-subscribe to new MapGrid
        if (_vm?.MapGrid != null)
            _vm.MapGrid.MapChanged += OnMapChanged;

        _tileBrushCache.Clear();
        _entityBrushCache.Clear();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void OnMapChanged() => InvalidateVisual();

    // ── Layout ────────────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        var grid = _vm?.MapGrid;
        if (grid == null) return new Size(400, 300);
        return new Size(grid.MapFile.Size.Width  * grid.CellSize,
                        grid.MapFile.Size.Height * grid.CellSize);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var vm   = _vm;
        var grid = vm?.MapGrid;

        if (vm == null || grid == null)
        {
            ctx.DrawRectangle(new SolidColorBrush(Color.Parse("#1E1E2E")), null,
                              new Rect(Bounds.Size));
            DrawCenteredText(ctx, "Aucune map ouverte", Brushes.Gray, Bounds.Size);
            return;
        }

        var cs  = grid.CellSize;
        var map = grid.MapFile;

        for (int y = 0; y < map.Size.Height; y++)
        {
            for (int x = 0; x < map.Size.Width; x++)
            {
                var rect = new Rect(x * cs, y * cs, cs, cs);

                // ── Tile background ──
                var tileType = grid.GetTileTypeAt(x, y) ?? grid.DefaultTileType;
                ctx.DrawRectangle(GetTileBrush(tileType), null, rect);

                // ── Grid lines ──
                ctx.DrawRectangle(null, GridPen, rect);

                // ── Entity overlay ──
                var entity = grid.GetEntityAt(x, y);
                if (entity != null)
                {
                    var entityType = vm.ActiveModule?.FindEntityType(entity.EntityTypeId);
                    if (entityType != null)
                        DrawEntity(ctx, entityType, rect, cs);
                }

                // ── Hover ──
                if (x == grid.HoverX && y == grid.HoverY)
                    ctx.DrawRectangle(HoverBrush, null, rect);

                // ── Selection ──
                if (x == grid.SelectedX && y == grid.SelectedY)
                    ctx.DrawRectangle(null, SelectionPen, rect);
            }
        }
    }

    private void DrawEntity(DrawingContext ctx, EntityTypeDefinition entityType,
                             Rect cellRect, int cs)
    {
        var brush  = GetEntityBrush(entityType);
        var radius = cs * 0.35;
        var center = cellRect.Center;
        ctx.DrawEllipse(brush, null, center, radius, radius);

        var letter = entityType.Name.Length > 0 ? entityType.Name[0].ToString() : "?";
        var ft = new FormattedText(
            letter,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            EntityFont,
            cs * 0.4,
            Brushes.White);

        ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
    }

    private static void DrawCenteredText(DrawingContext ctx, string text,
                                          IBrush brush, Size bounds)
    {
        var ft = new FormattedText(text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("default"), 16, brush);
        ctx.DrawText(ft, new Point((bounds.Width  - ft.Width)  / 2,
                                   (bounds.Height - ft.Height) / 2));
    }

    // ── Brush helpers ─────────────────────────────────────────────────────────

    private IBrush GetTileBrush(TileTypeDefinition td)
    {
        if (_tileBrushCache.TryGetValue(td.Id, out var cached)) return cached;
        IBrush brush;
        if (!string.IsNullOrEmpty(td.Color))
            brush = new SolidColorBrush(Color.Parse(td.Color));
        else
            brush = td.IsWall ? FallbackWall : FallbackFloor;
        _tileBrushCache[td.Id] = brush;
        return brush;
    }

    private IBrush GetEntityBrush(EntityTypeDefinition ed)
    {
        if (_entityBrushCache.TryGetValue(ed.Id, out var cached)) return cached;
        IBrush brush;
        if (!string.IsNullOrEmpty(ed.Color))
            brush = new SolidColorBrush(Color.Parse(ed.Color));
        else
            brush = ed.Category switch
            {
                "SPAWN" => SpawnBrush,
                "NPC"   => ed.Faction == "ENEMY" ? UnknownBrush : NpcBrush,
                "ITEM"  => ItemBrush,
                _       => UnknownBrush
            };
        _entityBrushCache[ed.Id] = brush;
        return brush;
    }

    // ── Mouse handling ────────────────────────────────────────────────────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_vm?.MapGrid == null) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _isPressed = true;
        var (x, y) = ToCell(e.GetPosition(this));
        _vm.HandleCellInteraction(x, y, isDrag: false);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_vm?.MapGrid == null) return;
        var (x, y) = ToCell(e.GetPosition(this));
        _vm.HandleHover(x, y);
        if (_isPressed) _vm.HandleCellInteraction(x, y, isDrag: true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPressed = false;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _vm?.HandleHover(null, null);
        _isPressed = false;
    }

    private (int x, int y) ToCell(Point p)
    {
        var cs = _vm?.MapGrid?.CellSize ?? 32;
        return ((int)(p.X / cs), (int)(p.Y / cs));
    }
}
