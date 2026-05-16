namespace DungeonCrawler.RaylibGame;

public enum AnimType
{
    None,
    Forward, Backward,
    TurnLeft, TurnRight,
    StrafeLeft, StrafeRight
}

/// <summary>
/// Suit la progression d'une animation de déplacement (0 → 1).
/// </summary>
public class AnimationState
{
    private const float Duration = 0.13f;   // secondes

    public AnimType Type     { get; private set; } = AnimType.None;
    public float    Progress { get; private set; } = 0f;
    public bool     IsPlaying => Type != AnimType.None;

    public void Start(AnimType type)
    {
        Type     = type;
        Progress = 0f;
    }

    public void Update(float dt)
    {
        if (!IsPlaying) return;
        Progress += dt / Duration;
        if (Progress >= 1f) { Progress = 1f; Type = AnimType.None; }
    }
}
