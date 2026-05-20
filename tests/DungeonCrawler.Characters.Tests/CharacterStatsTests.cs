using DungeonCrawler.Characters.Backgrounds;
using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using FluentAssertions;
using System.Xml.Linq;
using Xunit;

namespace DungeonCrawler.Characters.Tests;

public class CharacterStatsTests
{
    private static Character MakeCharacter(
        int musculature = 0, int flexibility = 0,
        int brain = 0, int vitality = 0)
    {
        var attrs = new CharacterAttributes(
            new CharacterAttribute(musculature),
            new CharacterAttribute(flexibility),
            new CharacterAttribute(brain),
            new CharacterAttribute(vitality));

        var desc = new CharacterDescription(
            Name: new CharacterName("Test", "Hero"),
            Age: 20,
            Gender: CharacterGender.Male,
            Size: CharacterSize.Medium,
            Weight: CharacterWeight.Average,
            Sensitivity: CharacterSensitivity.Normal,
            Background: null);

        return Character.Create(desc, attrs);
    }

    // ── MaxHp ─────────────────────────────────────────────────────────────────

    [Fact]
    public void MaxHp_BaseCharacter_EqualsBasePv()
    {
        var c = MakeCharacter();
        c.MaxHp().Should().Be(Character.BasePv); // 0×2 + 0 + 15 = 15
    }

    [Fact]
    public void MaxHp_Formula_VitalityX2_PlusMusculature_PlusBase()
    {
        var c = MakeCharacter(musculature: 3, vitality: 4);
        c.MaxHp().Should().Be(4 * 2 + 3 + Character.BasePv); // 8+3+15 = 26
    }

    // ── Quotients d'attaque ───────────────────────────────────────────────────

    [Fact]
    public void MightyAttack_Formula_MusculatureX2_PlusVitality()
    {
        var c = MakeCharacter(musculature: 3, vitality: 2);
        c.MightyAttackQuotient().Should().Be(3 * 2 + 2); // 8
    }

    [Fact]
    public void CriticalAttack_Formula_BrainX2_PlusFlexibility()
    {
        var c = MakeCharacter(brain: 4, flexibility: 1);
        c.CriticalAttackQuotient().Should().Be(4 * 2 + 1); // 9
    }

    // ── Quotients de défense ──────────────────────────────────────────────────

    [Fact]
    public void ParryDefense_Formula_VitalityX2_PlusMusculature()
    {
        var c = MakeCharacter(vitality: 3, musculature: 2);
        c.ParryDefenseQuotient().Should().Be(3 * 2 + 2); // 8
    }

    [Fact]
    public void DodgeDefense_Formula_FlexibilityX2_PlusBrain()
    {
        var c = MakeCharacter(flexibility: 2, brain: 3);
        c.DodgeDefenseQuotient().Should().Be(2 * 2 + 3); // 7
    }

    // ── CharacterState ────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_HpEqualsMaxHp()
    {
        var c = MakeCharacter(vitality: 2);
        c.State.CurrentHp.Should().Be(c.MaxHp());
    }

    [Fact]
    public void WithDamage_ReducesHp()
    {
        var c = MakeCharacter();
        var damaged = c.WithDamage(5);
        damaged.State.CurrentHp.Should().Be(c.MaxHp() - 5);
    }

    [Fact]
    public void WithDamage_CannotGoBelowZero()
    {
        var c = MakeCharacter();
        var damaged = c.WithDamage(999);
        damaged.State.CurrentHp.Should().Be(0);
        damaged.State.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void WithHeal_RestoresHp()
    {
        var c = MakeCharacter();
        var damaged = c.WithDamage(10);
        var healed = damaged.WithHeal(6);
        healed.State.CurrentHp.Should().Be(c.MaxHp() - 4);
    }

    [Fact]
    public void WithHeal_CannotExceedMaxHp()
    {
        var c = MakeCharacter();
        var healed = c.WithHeal(999);
        healed.State.CurrentHp.Should().Be(c.MaxHp());
    }

    [Fact]
    public void WithInjury_AddsToInjuryList()
    {
        var c = MakeCharacter();
        var injury = new Injury.Physical.Cut(InjurySeverity.Minor, Injury.BodyLocation.Torso);
        var result = c.WithInjury(injury);

        result.State.HasInjuries.Should().BeTrue();
        result.State.Injuries.Should().ContainSingle();
    }
}