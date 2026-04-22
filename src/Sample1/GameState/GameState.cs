namespace Sample1.GameState;

public enum PlayerRank
{
    Civil,
    Soldat,
    Officier,
    Déserteur
}

/// <summary>État mutable du jeu, partagé entre les scripts et le jeu.</summary>
public sealed class GameState
{
    public string     PlayerName { get; set; } = "Joueur";
    public PlayerRank Rank       { get; set; } = PlayerRank.Civil;

    // Flags modifiables par les conséquences
    public bool HasPass          { get; set; } = false;
    public bool DoorOpen         { get; set; } = false;
    public bool Alarme           { get; set; } = false;
    public bool MessageDelivered { get; set; } = false;
    public bool PlayerFled       { get; set; } = false;
}
