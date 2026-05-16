namespace DungeonCrawler.Core.Models;

public class DungeonMap
{
    private readonly Tile[,] _tiles;

    public int    Width  { get; }
    public int    Height { get; }
    public string Name   { get; set; } = string.Empty;
    public int    Level  { get; set; } = 1;

    // ── Construction ──────────────────────────────────────────────────────────

    public DungeonMap(int width, int height, string name = "")
    {
        Width  = width;
        Height = height;
        Name   = name;
        _tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            _tiles[x, y] = Tile.Floor();
    }

    // ── Tile access ───────────────────────────────────────────────────────────

    public bool IsInBounds(int x, int y)    => x >= 0 && x < Width && y >= 0 && y < Height;
    public bool IsInBounds(GridPosition pos) => IsInBounds(pos.X, pos.Y);

    public Tile? GetTile(int x, int y)       => IsInBounds(x, y) ? _tiles[x, y] : null;
    public Tile? GetTile(GridPosition pos)   => GetTile(pos.X, pos.Y);

    public void SetTile(int x, int y, Tile tile)
    {
        if (IsInBounds(x, y)) _tiles[x, y] = tile;
    }
    public void SetTile(GridPosition pos, Tile tile) => SetTile(pos.X, pos.Y, tile);

    /// <summary>Out-of-bounds positions are treated as solid.</summary>
    public bool IsSolid(GridPosition pos)    => GetTile(pos)?.IsSolid ?? true;
    public bool IsPassable(GridPosition pos) => !IsSolid(pos);

    // ── Batch helpers ─────────────────────────────────────────────────────────

    /// <summary>Fill a rectangle with copies of the given tile.</summary>
    public void Fill(int x, int y, int w, int h, Func<Tile> factory)
    {
        for (int ix = x; ix < x + w; ix++)
        for (int iy = y; iy < y + h; iy++)
            SetTile(ix, iy, factory());
    }

    /// <summary>Surround the entire map with a wall border.</summary>
    public void FillBorder()
    {
        for (int x = 0; x < Width;  x++) { SetTile(x, 0,          Tile.Wall()); SetTile(x, Height - 1, Tile.Wall()); }
        for (int y = 0; y < Height; y++) { SetTile(0, y,          Tile.Wall()); SetTile(Width - 1, y,  Tile.Wall()); }
    }
}
