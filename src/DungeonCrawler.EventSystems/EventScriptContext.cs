using DungeonCrawler.Persistence;

namespace DungeonCrawler.EventSystems;

public class EventScriptContext
{
    private readonly List<IGameAction> _pending = new();

    // ── Infos de trigger ──────────────────────────────────────────────────────

    public WorldState World { get; }
    public EventTrigger Trigger { get; }
    public string? MapId { get; }
    public GridPos? PlayerPos { get; }
    public string? EntityId { get; }
    public int Turn { get; }

    public EventScriptContext(WorldState world, EventTrigger trigger,
                               string? mapId, GridPos? playerPos,
                               string? entityId, int turn)
    {
        World = world;
        Trigger = trigger;
        MapId = mapId;
        PlayerPos = playerPos;
        EntityId = entityId;
        Turn = turn;
    }

    // ── API WorldState ────────────────────────────────────────────────────────

    public void SetFlag(string flag) => World.SetFlag(flag);
    public void ClearFlag(string flag) => World.ClearFlag(flag);
    public bool HasFlag(string flag) => World.HasFlag(flag);

    public int GetVar(string key, int defaultValue = 0) => World.GetVariable(key, defaultValue);
    public void SetVar(string key, int value) => World.SetVariable(key, value);
    public void IncrVar(string key, int amount = 1) => World.IncrementVariable(key, amount);

    public NpcState Npc(string npcId) => World.GetOrCreateNpc(npcId);
    public bool IsNpcAlive(string npcId) => World.IsNpcAlive(npcId);

    // ── API actions différées ─────────────────────────────────────────────────

    public void StartDialogue(string dialogueId) => Enqueue(new StartDialogueAction(dialogueId));
    public void GiveItem(string itemId, int qty = 1) => Enqueue(new GiveItemAction(itemId, qty));
    public void StartCombat(string encounterId) => Enqueue(new StartCombatAction(encounterId));
    public void ShowMessage(string message) => Enqueue(new ShowMessageAction(message));
    public void Recruit(string characterId) => Enqueue(new RecruitAction(characterId));
    public void RemoveEntity(string entityId) => Enqueue(new RemoveEntityAction(entityId));

    // ── Lecture des actions ───────────────────────────────────────────────────

    public IReadOnlyList<IGameAction> PendingActions => _pending;
    public bool HasActions => _pending.Count > 0;

    private void Enqueue(IGameAction action) => _pending.Add(action);
}