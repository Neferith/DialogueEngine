using DungeonCrawler.EventSystems;
using DungeonCrawler.Persistence;

namespace DungeonCrawler.EventSystems;

public class EventContext
{
    public string? CurrentMapId { get; init; }
    public int TurnNumber { get; init; }
    public GridPos? PlayerPos { get; init; }
    public string? EntityId { get; init; }
}

public record GridPos(int X, int Y);

public class EventSystem
{
    private readonly List<GameEvent> _events;
    private readonly EventScriptRegistry _scriptRegistry;

    public event Action<GameEvent, IReadOnlyList<IGameAction>>? EventFired;

    public EventSystem(EventScriptRegistry scriptRegistry)
    {
        _events = new();
        _scriptRegistry = scriptRegistry;
    }

    // ── Enregistrement ────────────────────────────────────────────────────────

    public void Register(IEnumerable<GameEvent> events)
    {
        _events.AddRange(events);
    }

    public void Register(GameEvent gameEvent)
    {
        _events.Add(gameEvent);
    }

    // ── Déclenchement ─────────────────────────────────────────────────────────

    public void Trigger(EventTrigger trigger, WorldState world, EventContext ctx)
    {
        var candidates = _events.Where(e =>
            e.Trigger == trigger &&
            MatchesContext(e, ctx) &&
            ConditionMet(e.Condition, world, ctx));

        foreach (var ev in candidates)
            ApplyEffects(ev, world, ctx);  // ← plus de EventFired ici
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static bool MatchesContext(GameEvent ev, EventContext ctx) =>
        ev.Trigger switch
        {
            EventTrigger.GameStart => true,
            EventTrigger.TurnPassed => true,
            EventTrigger.MapEnter => ev.MapId == null || ev.MapId == ctx.CurrentMapId,
            EventTrigger.TileEnter => ev.MapId == ctx.CurrentMapId &&
                                        ev.TileX == ctx.PlayerPos?.X &&
                                        ev.TileY == ctx.PlayerPos?.Y,
            EventTrigger.Interact => ev.EntityId == ctx.EntityId,
            EventTrigger.Proximity => ev.MapId == ctx.CurrentMapId &&
                                        ctx.PlayerPos != null &&
                                        ev.TileX != null && ev.TileY != null &&
                                        Distance(ctx.PlayerPos, ev.TileX.Value, ev.TileY.Value)
                                            <= (ev.Radius ?? 1),
            _ => false
        };

    private static bool ConditionMet(EventCondition cond, WorldState world,
                                      EventContext ctx)
    {
        if (cond.FlagNotSet != null && world.HasFlag(cond.FlagNotSet)) return false;
        if (cond.FlagSet != null && !world.HasFlag(cond.FlagSet)) return false;
        if (cond.NpcAlive != null && !world.IsNpcAlive(cond.NpcAlive)) return false;
        if (cond.NpcNotAlive != null && world.IsNpcAlive(cond.NpcNotAlive)) return false;
        if (cond.MinTurn != null && ctx.TurnNumber < cond.MinTurn) return false;
        return true;
    }

    private void ApplyEffects(GameEvent ev, WorldState world, EventContext ctx)
    {
        var scriptCtx = new EventScriptContext(
            world, ev.Trigger, ctx.CurrentMapId,
            ctx.PlayerPos, ctx.EntityId, ctx.TurnNumber);

        foreach (var effect in ev.Effects)
            _scriptRegistry.Execute(effect.ScriptId, scriptCtx, effect.Params);

        if (scriptCtx.HasActions)
            EventFired?.Invoke(ev, scriptCtx.PendingActions);
    }

    private static float Distance(GridPos pos, int x, int y) =>
        MathF.Sqrt(MathF.Pow(pos.X - x, 2) + MathF.Pow(pos.Y - y, 2));
}