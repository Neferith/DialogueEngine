using DungeonCrawler.Core;

namespace DungeonCrawler.Persistence;

public class WorldState
{
    public HashSet<string> Flags { get; set; } = new();
    public Dictionary<string, int> Variables { get; set; } = new();
    public Dictionary<string, NpcState> Npcs { get; set; } = new();

    public int TurnNumber { get; set; } = 0;

    /// Clé1 : mapId  Clé2 : "x_y"  Valeur : items (itemId → quantité)
    public Dictionary<string, Dictionary<string, Dictionary<string, int>>> TileInventoryOverrides { get; set; } = new();

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


    public void SetTileInventory(string mapId, int x, int y, Inventory inventory)
    {
        if (!TileInventoryOverrides.ContainsKey(mapId))
            TileInventoryOverrides[mapId] = new();

        // Toujours stocker — vide = "les items ont été retirés"
        TileInventoryOverrides[mapId][$"{x}_{y}"] =
            new Dictionary<string, int>(inventory.Items);
    }

    public bool TryGetTileInventory(string mapId, int x, int y,
                                     out Dictionary<string, int>? items)
    {
        items = null;
        return TileInventoryOverrides.TryGetValue(mapId, out var map)
            && map.TryGetValue($"{x}_{y}", out items);
    }
}