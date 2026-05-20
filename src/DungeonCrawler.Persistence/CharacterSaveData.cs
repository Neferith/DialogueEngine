namespace DungeonCrawler.Persistence;

public class CharacterSaveData
{
    // ── Identité ──────────────────────────────────────────────────────────────
    public string Id { get; set; } = "";
    public string FactionId { get; set; } = "";
    public string Firstname { get; set; } = "";
    public string Lastname { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public string Size { get; set; } = "";
    public string Weight { get; set; } = "";
    public string Sensitivity { get; set; } = "";
    public string BackgroundId { get; set; } = "";

    // ── Attributs (valeurs permanentes) ──────────────────────────────────────
    public int Musculature { get; set; }
    public int Flexibility { get; set; }
    public int Brain { get; set; }
    public int Vitality { get; set; }

    // ── État ──────────────────────────────────────────────────────────────────
    public int CurrentHp { get; set; }
    public List<InjurySaveData> Injuries { get; set; } = new();

    // ── Progression ───────────────────────────────────────────────────────────
    public int Level { get; set; } = 1;
    public int Experience { get; set; }

    // ── Compétences ───────────────────────────────────────────────────────────
    public List<string> SkillIds { get; set; } = new();
}

public class InjurySaveData
{
    public string Severity { get; set; } = "";  // Minor / Moderate / Severe
    public string Family { get; set; } = "";  // Physical / Mental / Energy
    public string SubType { get; set; } = "";  // Cut / Fracture / Shock…
    public string Detail { get; set; } = "";  // Location / Effect / Source
}