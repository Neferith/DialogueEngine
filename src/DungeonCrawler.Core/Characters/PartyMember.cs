namespace DungeonCrawler.Core.Characters;

/// <summary>
/// A party member. Stats are intentionally minimal at this stage;
/// the RPG stat system (ported from Kotlin) will be added separately.
/// </summary>
public class PartyMember
{
    public string Name    { get; set; }
    public bool   IsAlive { get; set; } = true;

    public PartyMember(string name)
    {
        Name = name;
    }

    public override string ToString() => $"{Name}{(IsAlive ? "" : " [KO]")}";
}
