using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Rendering;

/// <summary>
/// Builds a DungeonView snapshot from the current party position/facing and the map.
/// Stateless — call Build() every frame / after every action.
/// </summary>
public class ViewBuilder
{
    private readonly ViewConfig _config;

    public ViewBuilder(ViewConfig? config = null)
    {
        _config = config ?? new ViewConfig();
    }

    public DungeonView Build(Party party, DungeonMap map)
    {
        var cells = new List<VisibleCell>();
        var pos = party.Position;
        var facing = party.Facing;
        var forward = facing.ToOffset();
        var right = facing.TurnRight().ToOffset();

        for (int d = 1; d <= _config.MaxDepth; d++)
        {
            int spread = _config.GetSpread(d);

            for (int lat = -spread; lat <= spread; lat++)
            {
                var cellPos = new GridPosition(
                    pos.X + forward.X * d + right.X * lat,
                    pos.Y + forward.Y * d + right.Y * lat
                );

                var tile = map.GetTile(cellPos);
                if (tile == null) continue;

                var faceTowardPlayer = lat switch
                {
                    0 => facing.Opposite(),
                    < 0 => facing.TurnRight(),
                    _ => facing.TurnLeft()
                };

                cells.Add(new VisibleCell(cellPos, d, lat, tile, faceTowardPlayer));
            }
        }

        // ── Interaction target (tile directly ahead) ──────────────────────────
        var frontPos = pos + facing.ToOffset();
        var frontTile = map.GetTile(frontPos) ?? new Tile();

        var type = frontTile switch
        {
            { IsSolid: true, Tag: TileTag.Door } => InteractionType.Door,
            { IsSolid: true } => InteractionType.Wall,
            { Tag: TileTag.StairsUp } => InteractionType.StairsUp,
            { Tag: TileTag.StairsDown } => InteractionType.StairsDown,
            _ => InteractionType.None
        };

        var target = new InteractionTarget(frontPos, type, frontTile);

        return new DungeonView(pos, facing, cells, [], target);
    }
}