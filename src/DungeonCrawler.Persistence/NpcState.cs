namespace DungeonCrawler.Persistence;

public class NpcState
{
    public string NpcId { get; set; } = "";
    public int Hostility { get; set; } = 0;   // 0–100
    public int Affinity { get; set; } = 0;   // 0–100
    public bool IsAlive { get; set; } = true;
    public bool IsRecruited { get; set; } = false;
    public HashSet<string> Flags { get; set; } = new();
}