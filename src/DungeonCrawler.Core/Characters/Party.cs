using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Characters;

public class Party
{
    private readonly List<PartyMember> _members = new();

    /// <summary>Current tile on the map.</summary>
    public GridPosition Position { get; internal set; }

    /// <summary>Direction the party is facing.</summary>
    public Direction Facing { get; internal set; }

    /// <summary>Maximum number of members. Default 4, configurable at creation.</summary>
    public int MaxSize { get; }

    public IReadOnlyList<PartyMember> Members    => _members;
    public int                        MemberCount => _members.Count;
    public bool                       IsFull      => _members.Count >= MaxSize;

    public Party(GridPosition startPosition, Direction startFacing, int maxSize = 4)
    {
        Position = startPosition;
        Facing   = startFacing;
        MaxSize  = maxSize;
    }

    // ── Roster management ─────────────────────────────────────────────────────

    /// <returns>false if the party is already full.</returns>
    public bool TryAddMember(PartyMember member)
    {
        if (IsFull) return false;
        _members.Add(member);
        return true;
    }

    public bool RemoveMember(PartyMember member) => _members.Remove(member);

    public IEnumerable<PartyMember> AliveMembers => _members.Where(m => m.IsAlive);

    public override string ToString() =>
        $"{Position} {Facing.ToArrow()} [{string.Join(", ", _members)}]";
}
