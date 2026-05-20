namespace DungeonCrawler.Persistence;

public class WorldState
{
    public HashSet<string> Flags { get; set; } = new();
    public Dictionary<string, int> Variables { get; set; } = new();
    public Dictionary<string, NpcState> Npcs { get; set; } = new();

    // ── Flags ─────────────────────────────────────────────────────────────────

    public bool HasFlag(string flag) => Flags.Contains(flag);
    public void SetFlag(string flag) => Flags.Add(flag);
    public void ClearFlag(string flag) => Flags.Remove(flag);

    // ── Variables ─────────────────────────────────────────────────────────────

    public int GetVariable(string key, int defaultValue = 0) =>
        Variables.TryGetValue(key, out var v) ? v : defaultValue;
    public void SetVariable(string key, int value) => Variables[key] = value;
    public void IncrementVariable(string key, int amount = 1) =>
        Variables[key] = GetVariable(key) + amount;

    // ── NPC ───────────────────────────────────────────────────────────────────

    public NpcState GetOrCreateNpc(string npcId)
    {
        if (!Npcs.TryGetValue(npcId, out var state))
        {
            state = new NpcState { NpcId = npcId };
            Npcs[npcId] = state;
        }
        return state;
    }

    public NpcState? GetNpc(string npcId) =>
        Npcs.TryGetValue(npcId, out var s) ? s : null;

    public bool IsNpcAlive(string npcId) =>
        GetNpc(npcId)?.IsAlive ?? true;

    public bool IsNpcRecruited(string npcId) =>
        GetNpc(npcId)?.IsRecruited ?? false;
}