namespace DungeonCrawler.Characters.Models;

public record CharacterState(
    int CurrentHp,
    List<Injury> Injuries = null!)
{
    public List<Injury> Injuries { get; init; } = Injuries ?? new();

    public bool IsAlive => CurrentHp > 0;
    public bool HasInjuries => Injuries.Count > 0;
    public bool HasSevereInjury =>
        Injuries.Any(i => i.Severity == InjurySeverity.Severe);

    public CharacterState WithDamage(int amount) =>
        this with { CurrentHp = Math.Max(0, CurrentHp - amount) };

    public CharacterState WithInjury(Injury injury) =>
        this with { Injuries = [.. Injuries, injury] };

    public CharacterState WithHeal(int amount, int maxHp) =>
        this with { CurrentHp = Math.Min(maxHp, CurrentHp + amount) };
}