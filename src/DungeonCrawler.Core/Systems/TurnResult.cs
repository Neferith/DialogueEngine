using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Systems;

// ── Party action ──────────────────────────────────────────────────────────────

public enum PartyActionType
{
    MoveForward, MoveBackward, StrafeLeft, StrafeRight,
    TurnLeft, TurnRight, Turn180,
    Interact, Wait
}

// ── Entity actions (what each monster did this turn) ─────────────────────────

public abstract record EntityAction(DungeonEntity Entity);

public record EntityWaited(DungeonEntity Entity) : EntityAction(Entity);

public record EntityMoved(DungeonEntity Entity, GridPosition From, GridPosition To)
    : EntityAction(Entity);

/// <summary>
/// Monster is adjacent and strikes. Damage resolution requires the RPG stat
/// system (Kotlin port, added later) — for now this is a pure event.
/// </summary>
public record EntityAttacked(DungeonEntity Entity, GridPosition Target)
    : EntityAction(Entity);

// ── Turn result ───────────────────────────────────────────────────────────────

public record TurnResult(
    PartyActionType PartyAction,
    MoveResult? MovementResult,
    bool InteractionTriggered
)
{
    public bool PartyMoved => MovementResult == MoveResult.Success;
    public bool WasBlocked => MovementResult is MoveResult.BlockedByWall
                                             or MoveResult.BlockedByEntity;
}
