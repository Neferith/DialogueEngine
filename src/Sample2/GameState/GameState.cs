namespace Sample2.GameState;

public enum Skill { Charme, Intimidation, Bluff }

public enum OfficerEmotion { None, Charmed, Scared, Convinced }

public sealed class GameState
{
    public string        PlayerName    { get; set; } = "Joueur";
    public Skill         PlayerSkill   { get; set; } = Skill.Charme;
    public bool          HasPass       { get; set; } = false;
    public bool          DoorOpen      { get; set; } = false;
    public bool          OfficerPassGiven { get; set; } = false;
    public OfficerEmotion OfficerEmotion { get; set; } = OfficerEmotion.None;
}
