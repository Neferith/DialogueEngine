namespace DungeonCrawler.Core.Models;

public enum MoveResult
{
    Success,
    BlockedByWall,
    BlockedByEntity,
    OutOfBounds
}
