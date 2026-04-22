using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Interfaces;
using Sample2.GameState;

namespace Sample2;

public sealed class GameContext : IDialogueContext
{
    public IVariableResolver Variables { get; }

    public GameContext(GameState.GameState state)
    {
        Variables = new Resolver(state);
    }

    private sealed class Resolver(GameState.GameState s) : IVariableResolver
    {
        public string Resolve(string key) => key switch
        {
            "player.name"  => s.PlayerName,
            "player.skill" => s.PlayerSkill.ToString(),
            _              => $"{{{key}}}"
        };
    }
}

public static class GameScripts
{
    public static ScriptRegistry CreateRegistry(GameState.GameState s) =>
        new ScriptRegistry()
            // Conditions — skill checks
            .Condition("has_charm",        _ => s.PlayerSkill == Skill.Charme)
            .Condition("has_intimidation", _ => s.PlayerSkill == Skill.Intimidation)
            .Condition("has_bluff",        _ => s.PlayerSkill == Skill.Bluff)
            // Conditions — state checks
            .Condition("has_pass",         _ => s.HasPass)
            .Condition("officer_charmed",  _ => s.OfficerEmotion == OfficerEmotion.Charmed)
            .Condition("officer_scared",   _ => s.OfficerEmotion == OfficerEmotion.Scared)
            // Conséquences
            .Consequence("set_charmed_and_give_pass",  _ => { s.HasPass = true; s.OfficerPassGiven = true; s.OfficerEmotion = OfficerEmotion.Charmed; })
            .Consequence("set_scared_and_give_pass",   _ => { s.HasPass = true; s.OfficerPassGiven = true; s.OfficerEmotion = OfficerEmotion.Scared; })
            .Consequence("set_convinced_and_give_pass",_ => { s.HasPass = true; s.OfficerPassGiven = true; s.OfficerEmotion = OfficerEmotion.Convinced; })
            .Consequence("open_door",                  _ => s.DoorOpen = true);
}
