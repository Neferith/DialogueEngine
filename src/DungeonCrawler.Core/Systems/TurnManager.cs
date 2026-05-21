using DungeonCrawler.Core;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;

public class TurnManager
{
    private readonly DungeonRunner _runner;

    /// <summary>Fires quand un vrai tour s'écoule.</summary>
    public event Action? TurnAdvanced;

    public TurnManager(DungeonRunner runner)
    {
        _runner = runner;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public TurnResult ExecuteAction(PartyActionType action)
    {
        MoveResult? moveResult = null;
        bool interactionTriggered = false;
        bool turnAdvanced = false;

        switch (action)
        {
            case PartyActionType.MoveForward:
                moveResult = _runner.MoveInDirection(_runner.Party.Facing);
                turnAdvanced = true;
                break;
            case PartyActionType.MoveBackward:
                moveResult = _runner.MoveInDirection(_runner.Party.Facing.Opposite());
                turnAdvanced = true;
                break;
            case PartyActionType.StrafeLeft:
                moveResult = _runner.MoveInDirection(_runner.Party.Facing.TurnLeft());
                turnAdvanced = true;
                break;
            case PartyActionType.StrafeRight:
                moveResult = _runner.MoveInDirection(_runner.Party.Facing.TurnRight());
                turnAdvanced = true;
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
                turnAdvanced = true;
                break;
            case PartyActionType.Wait:
                turnAdvanced = true;
                break;
        }

        if (turnAdvanced)
            AdvanceTurn();

        return new TurnResult(action, moveResult, interactionTriggered);
    }

    /// <summary>
    /// Mock : appelé depuis PlayingScreen (touche X) pour simuler
    /// qu'un membre a agi. Quand tous ont agi, le tour avance.
    /// </summary>
    public void NotifyMemberActed(string characterId)
    {
        var member = _runner.Party.Members
            .FirstOrDefault(m => m.CharacterId == characterId);
        if (member == null || !member.IsAlive || member.HasActed) return;

        member.HasActed = true;

        if (_runner.Party.AliveMembers.All(m => m.HasActed))
            AdvanceTurn();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void AdvanceTurn()
    {
        foreach (var m in _runner.Party.Members)
            m.HasActed = false;

        TurnAdvanced?.Invoke();
    }

    private bool HandleInteraction()
    {
        var view = _runner.GetView();
        if (view.FacingTarget == null) return false;

        switch (view.FacingTarget.Type)
        {
            case InteractionType.Door:
                view.FacingTarget.Tile.IsSolid = false;
                view.FacingTarget.Tile.Tag = TileTag.DoorOpen;
                return true;
            default:
                return true;
        }
    }
}