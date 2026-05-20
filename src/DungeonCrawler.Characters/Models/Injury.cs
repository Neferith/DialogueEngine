namespace DungeonCrawler.Characters.Models;

public enum InjurySeverity { Minor = 1, Moderate = 2, Severe = 3 }

public abstract record Injury(InjurySeverity Severity)
{
    // ── Physiques ─────────────────────────────────────────────────────────────

    public enum BodyLocation { Head, ArmLeft, ArmRight, LegLeft, LegRight, Torso }

    public abstract record Physical(InjurySeverity Severity, BodyLocation Location)
        : Injury(Severity)
    {
        public record Cut(InjurySeverity Severity, BodyLocation Location)
            : Physical(Severity, Location);
        public record Fracture(InjurySeverity Severity, BodyLocation Location)
            : Physical(Severity, Location);
        public record Burn(InjurySeverity Severity, BodyLocation Location)
            : Physical(Severity, Location);
    }

    // ── Mentales ──────────────────────────────────────────────────────────────

    public enum MentalEffect { Trauma, Panic, Confusion, Paranoia }

    public abstract record Mental(InjurySeverity Severity, MentalEffect Effect)
        : Injury(Severity)
    {
        public record Shock(InjurySeverity Severity, MentalEffect Effect)
            : Mental(Severity, Effect);
        public record Madness(InjurySeverity Severity, MentalEffect Effect)
            : Mental(Severity, Effect);
    }

    // ── Énergétiques ──────────────────────────────────────────────────────────

    public enum EnergySource { Fire, Electric, Cold, Radiation, Sonic }

    public abstract record Energy(InjurySeverity Severity, EnergySource Source)
        : Injury(Severity)
    {
        public record EnergyBurn(InjurySeverity Severity, EnergySource Source)
            : Energy(Severity, Source);
        public record Corruption(InjurySeverity Severity, EnergySource Source)
            : Energy(Severity, Source);
    }
}