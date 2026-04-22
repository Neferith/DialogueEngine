using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Interfaces;
using Sample1.GameState;

namespace Sample1;

public sealed class GameContext : IDialogueContext
{
    public IVariableResolver Variables { get; }

    public GameContext(GameState.GameState state)
    {
        Variables = new GameVariableResolver(state);
    }

    private sealed class GameVariableResolver : IVariableResolver
    {
        private readonly GameState.GameState _state;
        public GameVariableResolver(GameState.GameState state) => _state = state;

        public string Resolve(string key) => key switch
        {
            "player.name" => _state.PlayerName,
            "player.rank" => _state.Rank.ToString(),
            _             => $"{{{key}}}"
        };
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public static class GameScripts
{
    /// <summary>Crée et câble tous les scripts de condition et de conséquence.</summary>
    public static ScriptRegistry CreateRegistry(GameState.GameState state) =>
        new ScriptRegistry()
            // Conditions
            .Condition("rank_officer",    _ => state.Rank == PlayerRank.Officier)
            .Condition("rank_deserter",   _ => state.Rank == PlayerRank.Déserteur)
            .Condition("rank_soldier",    _ => state.Rank == PlayerRank.Soldat)
            .Condition("player_has_pass", _ => state.HasPass)
            // Conséquences
            .Consequence("unlock_door",        _ => state.DoorOpen         = true)
            .Consequence("consume_pass",       _ => state.HasPass          = false)
            .Consequence("vance_agace",        _ => { /* relation tracking possible */ })
            .Consequence("vance_alarme",       _ => state.Alarme           = true)
            .Consequence("player_flee",        _ => state.PlayerFled       = true)
            .Consequence("message_delivered",  _ => state.MessageDelivered = true);
}
