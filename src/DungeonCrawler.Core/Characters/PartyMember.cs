namespace DungeonCrawler.Core.Characters;

/// <summary>
/// A party member. Stats are intentionally minimal at this stage;
/// the RPG stat system (ported from Kotlin) will be added separately.
/// </summary>
public class PartyMember
{
    public string CharacterId { get; }
    public bool IsAlive { get; set; } = true;
    public bool HasActed { get; set; } = false;

    public PartyMember(string characterId)
    {
        CharacterId = characterId;
    }

    public override string ToString() => $"{CharacterId}{(IsAlive ? "" : " [KO]")}";
}