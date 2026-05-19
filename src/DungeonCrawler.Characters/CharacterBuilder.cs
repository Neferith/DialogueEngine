using DungeonCrawler.Characters.Backgrounds;
using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.Characters.Skills;

namespace DungeonCrawler.Characters;

public enum CreationStep
{
    Name = 0,
    Gender = 1,
    Size = 2,
    Weight = 3,
    Sensitivity = 4,
    Background = 5
}

public class CharacterBuilder
{
    private readonly List<BackgroundType> _backgroundTypes;
    private readonly List<Background> _selectedBackgrounds = new();

    // ── Étape courante ────────────────────────────────────────────────────────

    public CreationStep CurrentStep { get; private set; } = CreationStep.Name;

    // ── Choix du joueur ───────────────────────────────────────────────────────

    public string Firstname { get; private set; } = "";
    public string Lastname { get; private set; } = "";
    public CharacterGender? Gender { get; private set; }
    public CharacterSize? Size { get; private set; }
    public CharacterWeight? Weight { get; private set; }
    public CharacterSensitivity? Sensitivity { get; private set; }
    public Background? CurrentBackground { get; private set; }

    // ── Attributs accumulés ───────────────────────────────────────────────────

    public CharacterAttributes CurrentAttributes { get; private set; }
        = CharacterAttributes.Empty;

    // ── Backgrounds disponibles (type courant) ────────────────────────────────

    public BackgroundType? CurrentBackgroundType =>
        _selectedBackgrounds.Count < _backgroundTypes.Count
            ? _backgroundTypes[_selectedBackgrounds.Count]
            : null;

    public IReadOnlyList<Background> AvailableBackgrounds =>
        CurrentBackgroundType?.Backgrounds ?? [];

    // ── Options filtrées ──────────────────────────────────────────────────────

    public IReadOnlyList<CharacterSize> AvailableSizes =>
        Gender?.AvailableSizes() ?? [];
    public IReadOnlyList<CharacterWeight> AvailableWeights =>
        Size?.AvailableWeights() ?? [];
    public IReadOnlyList<CharacterSensitivity> AvailableSensitivities =>
        Weight?.AvailableSensitivities() ?? [];

    // ── État ──────────────────────────────────────────────────────────────────

    public bool IsLastStep =>
        CurrentStep == CreationStep.Background &&
        _selectedBackgrounds.Count == _backgroundTypes.Count - 1;

    public bool IsComplete =>
    CurrentStep == CreationStep.Background &&
    _selectedBackgrounds.Count == _backgroundTypes.Count;

    // ── Constructeur ──────────────────────────────────────────────────────────

    public CharacterBuilder(List<BackgroundType> backgroundTypes)
    {
        _backgroundTypes = backgroundTypes;
    }

    // ── Setters ───────────────────────────────────────────────────────────────

    public void SetFirstname(string v) => Firstname = v;
    public void SetLastname(string v) => Lastname = v;

    public void SetGender(CharacterGender v)
    {
        Gender = v;
        Size = null; Weight = null; Sensitivity = null;
    }

    public void SetSize(CharacterSize v)
    {
        Size = v;
        Weight = null; Sensitivity = null;
    }

    public void SetWeight(CharacterWeight v)
    {
        Weight = v;
        Sensitivity = null;
    }

    public void SetSensitivity(CharacterSensitivity v) => Sensitivity = v;
    public void SetBackground(Background v) => CurrentBackground = v;

    // ── Validation ────────────────────────────────────────────────────────────

    public bool IsCurrentStepValid() => CurrentStep switch
    {
        CreationStep.Name => Firstname.Trim().Length > 0 && Lastname.Trim().Length > 0,
        CreationStep.Gender => Gender != null,
        CreationStep.Size => Size != null,
        CreationStep.Weight => Weight != null,
        CreationStep.Sensitivity => Sensitivity != null,
        CreationStep.Background => CurrentBackground != null,
        _ => false
    };

    // ── Avancer ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Applique le modificateur de l'étape courante et avance.
    /// Ne fait rien si l'étape n'est pas valide.
    /// </summary>
    public void NextStep()
    {
        if (!IsCurrentStepValid()) return;

        ApplyCurrentModifier();

        if (CurrentStep == CreationStep.Background)
        {
            _selectedBackgrounds.Add(CurrentBackground!);
            CurrentBackground = null;

            // S'il reste des types de background → on reste sur l'étape Background
            if (_selectedBackgrounds.Count < _backgroundTypes.Count)
                return;
        }

        if (CurrentStep < CreationStep.Background)
            CurrentStep = (CreationStep)((int)CurrentStep + 1);
    }

    // ── Construction du personnage ────────────────────────────────────────────

    public Character Build(string factionId = "PLAYER_FACTION")
    {
        var desc = new CharacterDescription(
            Name: new CharacterName(Firstname.Trim(), Lastname.Trim()),
            Age: 20,
            Gender: Gender ?? CharacterGender.Male,
            Size: Size ?? CharacterSize.Medium,
            Weight: Weight ?? CharacterWeight.Average,
            Sensitivity: Sensitivity ?? CharacterSensitivity.Normal,
            Background: _selectedBackgrounds.LastOrDefault());

        return Character.Create(desc, CurrentAttributes, factionId);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void ApplyCurrentModifier()
    {
        AttributesModifier? mod = CurrentStep switch
        {
            CreationStep.Gender => Gender?.Modifier(),
            CreationStep.Size => Size?.Modifier(),
            CreationStep.Weight => Weight?.Modifier(),
            CreationStep.Sensitivity => Sensitivity?.Modifier(),
            CreationStep.Background => CurrentBackground?.AttributesModifier,
            _ => null
        };

        if (mod != null)
            CurrentAttributes = CurrentAttributes.ApplyModifier(mod);
    }
}