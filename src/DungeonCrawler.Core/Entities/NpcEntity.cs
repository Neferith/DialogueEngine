using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Entities;

/// <summary>
/// A non-hostile NPC. Blocks movement. Triggers dialogue on interaction.
/// DialogueFileId links to a DialogueEngine dialogue file.
/// </summary>
public class NpcEntity : DungeonEntity
{
    public string    Name           { get; set; }
    public string    DialogueFileId { get; set; }
    public Direction Facing         { get; set; }
    public override bool BlocksMovement => true;

    public NpcEntity(string id, GridPosition position, string name, string dialogueFileId,
                     Direction facing = Direction.South)
        : base(id, position)
    {
        Name           = name;
        DialogueFileId = dialogueFileId;
        Facing         = facing;
    }
}
