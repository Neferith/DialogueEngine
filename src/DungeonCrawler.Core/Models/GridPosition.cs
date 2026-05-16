namespace DungeonCrawler.Core.Models;

public readonly record struct GridPosition(int X, int Y)
{
    public static GridPosition operator +(GridPosition a, GridPosition b) =>
        new(a.X + b.X, a.Y + b.Y);

    public static GridPosition operator *(GridPosition a, int scalar) =>
        new(a.X * scalar, a.Y * scalar);

    public static GridPosition operator *(int scalar, GridPosition a) =>
        new(a.X * scalar, a.Y * scalar);

    public override string ToString() => $"({X}, {Y})";
}
