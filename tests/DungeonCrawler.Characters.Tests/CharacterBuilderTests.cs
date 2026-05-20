using DungeonCrawler.Characters.Backgrounds;
using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using FluentAssertions;
using Xunit;

namespace DungeonCrawler.Characters.Tests;

public class CharacterBuilderTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BackgroundType MakeBackgroundType(string id, params string[] backgroundIds) =>
        new()
        {
            Id = id,
            Name = id,
            Description = id,
            Backgrounds = backgroundIds.Select(bid => new Background
            {
                Id = bid,
                Name = bid,
                TypeId = id,
                AttributesModifier = AttributesModifier.Zero
            }).ToList()
        };

    private static CharacterBuilder MakeBuilder(int backgroundTypeCount = 1)
    {
        var types = Enumerable.Range(0, backgroundTypeCount)
            .Select(i => MakeBackgroundType($"type_{i}", $"bg_{i}_a", $"bg_{i}_b"))
            .ToList();
        return new CharacterBuilder(types);
    }

    private static void FillName(CharacterBuilder b)
    {
        b.SetFirstname("Aria");
        b.SetLastname("Voss");
    }

    private static void AdvanceThrough(CharacterBuilder b,
        CharacterGender gender = CharacterGender.Female,
        CharacterSize size = CharacterSize.Medium,
        CharacterWeight weight = CharacterWeight.Average,
        CharacterSensitivity sensitivity = CharacterSensitivity.Normal)
    {
        FillName(b); b.NextStep(); // Name → Gender
        b.SetGender(gender); b.NextStep(); // Gender → Size
        b.SetSize(size); b.NextStep(); // Size → Weight
        b.SetWeight(weight); b.NextStep(); // Weight → Sensitivity
        b.SetSensitivity(sensitivity); b.NextStep(); // Sensitivity → Background
    }

    // ── Étape initiale ────────────────────────────────────────────────────────

    [Fact]
    public void InitialStep_IsName()
    {
        var b = MakeBuilder();
        b.CurrentStep.Should().Be(CreationStep.Name);
    }

    [Fact]
    public void NextStep_DoesNothing_WhenStepInvalid()
    {
        var b = MakeBuilder();
        b.NextStep(); // name vide → invalide
        b.CurrentStep.Should().Be(CreationStep.Name);
    }

    // ── Progression ───────────────────────────────────────────────────────────

    [Fact]
    public void NextStep_AdvancesStep_WhenValid()
    {
        var b = MakeBuilder();
        FillName(b);
        b.NextStep();
        b.CurrentStep.Should().Be(CreationStep.Gender);
    }

    [Fact]
    public void FullFlow_ReachesBackground()
    {
        var b = MakeBuilder();
        AdvanceThrough(b);
        b.CurrentStep.Should().Be(CreationStep.Background);
    }

    // ── Filtrage des options ──────────────────────────────────────────────────

    [Fact]
    public void AvailableSizes_Empty_BeforeGenderSet()
    {
        var b = MakeBuilder();
        b.AvailableSizes.Should().BeEmpty();
    }

    [Fact]
    public void AvailableSizes_Filtered_ByGender()
    {
        var b = MakeBuilder();
        b.SetGender(CharacterGender.Female);
        b.AvailableSizes.Should().NotContain(CharacterSize.Giant);
        b.AvailableSizes.Should().NotContain(CharacterSize.Large);
    }

    [Fact]
    public void SetGender_Resets_SizeAndWeight()
    {
        var b = MakeBuilder();
        b.SetGender(CharacterGender.Male);
        b.SetSize(CharacterSize.Giant);
        b.SetGender(CharacterGender.Female);
        b.Size.Should().BeNull();
        b.Weight.Should().BeNull();
    }

    // ── Modificateurs accumulés ───────────────────────────────────────────────

    [Fact]
    public void Attributes_UpdateAfterGenderStep()
    {
        var b = MakeBuilder();
        FillName(b); b.NextStep();
        b.SetGender(CharacterGender.Male);
        b.NextStep();

        var mod = CharacterGender.Male.Modifier();
        b.CurrentAttributes.Musculature.Permanent.Should().BeInRange(0, mod.Musculature);
    }

    [Fact]
    public void Attributes_AccumulateAcrossSteps()
    {
        var b = MakeBuilder();
        AdvanceThrough(b,
            gender: CharacterGender.Male,
            size: CharacterSize.Giant,
            weight: CharacterWeight.Extreme,
            sensitivity: CharacterSensitivity.Insensitive);

        // Tous ces choix donnent des Musculature positifs → total forcément > 0
        b.CurrentAttributes.Musculature.Permanent.Should().BeGreaterThan(0);
    }

    // ── Background (type unique) ──────────────────────────────────────────────

    [Fact]
    public void IsLastStep_True_OnLastBackgroundType()
    {
        var b = MakeBuilder(backgroundTypeCount: 1);
        AdvanceThrough(b);
        b.IsLastStep.Should().BeTrue();
    }

    [Fact]
    public void Background_Completes_WithOneType()
    {
        var b = MakeBuilder(backgroundTypeCount: 1);
        AdvanceThrough(b);
        b.SetBackground(b.AvailableBackgrounds[0]);
        b.NextStep();
        b.IsComplete.Should().BeTrue();
    }

    // ── Background (types multiples) ──────────────────────────────────────────

    [Fact]
    public void Background_StaysOnStep_UntilAllTypesSelected()
    {
        var b = MakeBuilder(backgroundTypeCount: 2);
        AdvanceThrough(b);

        b.SetBackground(b.AvailableBackgrounds[0]);
        b.NextStep();

        b.CurrentStep.Should().Be(CreationStep.Background);
        b.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Background_Completes_AfterAllTypesSelected()
    {
        var b = MakeBuilder(backgroundTypeCount: 2);
        AdvanceThrough(b);

        b.SetBackground(b.AvailableBackgrounds[0]); b.NextStep();
        b.SetBackground(b.AvailableBackgrounds[0]); b.NextStep();

        b.IsComplete.Should().BeTrue();
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Build_ReturnsCharacter_WithCorrectName()
    {
        var b = MakeBuilder();
        AdvanceThrough(b);
        b.SetBackground(b.AvailableBackgrounds[0]);
        b.NextStep();

        var character = b.Build();
        character.Description.Name.Firstname.Should().Be("Aria");
        character.Description.Name.Lastname.Should().Be("Voss");
    }

    [Fact]
    public void Build_InitialHp_EqualsMaxHp()
    {
        var b = MakeBuilder();
        AdvanceThrough(b);
        b.SetBackground(b.AvailableBackgrounds[0]);
        b.NextStep();

        var character = b.Build();
        character.State.CurrentHp.Should().Be(character.MaxHp());
    }
}