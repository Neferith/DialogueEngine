using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Rendering;

/// <summary>
/// A single tile that falls inside the view frustum, annotated with its
/// position in the view (distance + lateral offset) and which of its faces
/// is oriented toward the player.
/// </summary>
public record VisibleCell(
    GridPosition MapPosition,

    /// <summary>1 = one step ahead, 2 = two steps ahead, etc.</summary>
    int Distance,

    /// <summary>Negative = left of center, 0 = straight ahead, positive = right.</summary>
    int LateralOffset,

    Tile Tile,

    /// <summary>
    /// Which face of this tile is turned toward the player.
    /// Used by the renderer to pick the correct wall texture / sprite.
    ///   Center column  → Opposite of player facing
    ///   Left column    → TurnRight of player facing  (east wall if facing north)
    ///   Right column   → TurnLeft  of player facing  (west wall if facing north)
    /// </summary>
    Direction FaceTowardPlayer
);

// ── Visible entities ──────────────────────────────────────────────────────────

/// <summary>An entity that falls inside the view frustum.</summary>
public record VisibleEntity(
    DungeonEntity Entity,
    int           Distance,
    int           LateralOffset
);

// ── Interaction target ────────────────────────────────────────────────────────

public enum InteractionType { None, Wall, Door, Npc, Item, StairsUp, StairsDown }

/// <summary>What is directly in front of the party (one step ahead).</summary>
public record InteractionTarget(
    GridPosition    Position,
    InteractionType Type,
    Tile            Tile,

    /// <summary>Set when Type is Npc or Item.</summary>
    DungeonEntity?  Entity = null
);

// ── The main snapshot ─────────────────────────────────────────────────────────

/// <summary>
/// Engine-agnostic description of what the party currently sees.
/// Built each frame by ViewBuilder; consumed by whichever renderer is attached.
/// </summary>
public record DungeonView(
    GridPosition               PartyPosition,
    Direction                  PartyFacing,
    IReadOnlyList<VisibleCell> Cells,

    /// <summary>Entities within the view frustum, for the renderer to draw.</summary>
    IReadOnlyList<VisibleEntity> VisibleEntities,

    /// <summary>
    /// What is immediately in front of the party (null if out of bounds).
    /// Used for interaction prompts.
    /// </summary>
    InteractionTarget? FacingTarget
);
