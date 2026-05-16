namespace DungeonCrawler.Core.Models;

/// <summary>
/// Cardinal directions. Order matters: N=0, E=1, S=2, W=3.
/// Turning right increments by 1 (mod 4).
/// </summary>
public enum Direction { North, East, South, West }

public static class DirectionExtensions
{
    public static Direction TurnRight(this Direction d) => (Direction)(((int)d + 1) % 4);
    public static Direction TurnLeft(this Direction d)  => (Direction)(((int)d + 3) % 4);
    public static Direction Opposite(this Direction d)  => (Direction)(((int)d + 2) % 4);

    /// <summary>Grid offset for one step in this direction. Y+ = North.</summary>
    public static GridPosition ToOffset(this Direction d) => d switch
    {
        Direction.North => new GridPosition( 0,  1),
        Direction.East  => new GridPosition( 1,  0),
        Direction.South => new GridPosition( 0, -1),
        Direction.West  => new GridPosition(-1,  0),
        _               => throw new ArgumentOutOfRangeException(nameof(d))
    };

    public static char ToArrow(this Direction d) => d switch
    {
        Direction.North => '↑',
        Direction.East  => '→',
        Direction.South => '↓',
        Direction.West  => '←',
        _               => '?'
    };
}
