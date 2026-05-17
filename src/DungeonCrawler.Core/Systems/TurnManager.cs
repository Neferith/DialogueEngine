using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;

namespace DungeonCrawler.Core.Systems;

/// <summary>
/// Orchestrates the turn cycle:
///   1. Party performs one action.
///   2. Every active monster performs one action.
///   3. TurnCompleted event fires with the full summary.
///
/// Use this as the main input point for the game loop instead of calling
/// DungeonRunner directly — it adds entity-blocking checks and sequences turns.
/// </summary>
public class TurnManager
{
    private readonly DungeonRunner _runner;
    private readonly EntitySystem  _entitySystem;

    public int TurnNumber { get; private set; }

    /// <summary>Fires after every complete turn (party + all entities).</summary>
    public event Action<TurnResult>? TurnCompleted;

    public TurnManager(DungeonRunner runner, EntitySystem entitySystem)
    {
        _runner       = runner;
        _entitySystem = entitySystem;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public TurnResult ExecuteAction(PartyActionType action)
    {
        TurnNumber++;

        MoveResult? moveResult           = null;
        bool        interactionTriggered = false;

        switch (action)
        {
            case PartyActionType.MoveForward:
                moveResult = TryMoveParty(_runner.Party.Facing);
                break;
            case PartyActionType.MoveBackward:
                moveResult = TryMoveParty(_runner.Party.Facing.Opposite());
                break;
            case PartyActionType.StrafeLeft:
                moveResult = TryMoveParty(_runner.Party.Facing.TurnLeft());
                break;
            case PartyActionType.StrafeRight:
                moveResult = TryMoveParty(_runner.Party.Facing.TurnRight());
                break;
            case PartyActionType.TurnLeft:
                _runner.TurnLeft();
                break;
            case PartyActionType.TurnRight:
                _runner.TurnRight();
                break;
            case PartyActionType.Turn180:
                _runner.Turn180();
                break;
            case PartyActionType.Interact:
                interactionTriggered = HandleInteraction();
                break;
            case PartyActionType.Wait:
                break;
        }

        var entityActions = ProcessEntityTurns();

        var result = new TurnResult(TurnNumber, action, moveResult, entityActions, interactionTriggered);
        TurnCompleted?.Invoke(result);
        return result;
    }

    // ── Party movement ────────────────────────────────────────────────────────

    private MoveResult TryMoveParty(Direction direction)
    {
        var target = _runner.Party.Position + direction.ToOffset();

        // Entity check before map check so we get the right result enum
        if (_entitySystem.HasBlockingEntity(target))
            return MoveResult.BlockedByEntity;

        return _runner.MoveInDirection(direction);
    }

    // ── Interaction ───────────────────────────────────────────────────────────

    private bool HandleInteraction()
    {
        var view = _runner.GetView();
        if (view.FacingTarget == null) return false;

        switch (view.FacingTarget.Type)
        {
            case InteractionType.Door:
                // Open the door: make the tile passable
                view.FacingTarget.Tile.IsSolid = false;
                view.FacingTarget.Tile.Tag     = TileTag.DoorOpen;
                return true;

            case InteractionType.Npc:
            case InteractionType.Item:
            case InteractionType.StairsUp:
            case InteractionType.StairsDown:
                // Signal the game layer to handle these (dialogue, pickup, floor change)
                return true;

            default:
                return false;
        }
    }

    // ── Entity turns ──────────────────────────────────────────────────────────

    private IReadOnlyList<EntityAction> ProcessEntityTurns()
    {
        var actions = new List<EntityAction>();

        // Snapshot the list so a behavior can't affect which monsters act this turn
        foreach (var monster in _entitySystem.GetAll<MonsterEntity>().ToList())
        {
            if (!monster.IsActive) continue;
            actions.Add(monster.Behavior.Act(monster, _entitySystem, _runner.Map, _runner.Party));
        }

        return actions;
    }
}
