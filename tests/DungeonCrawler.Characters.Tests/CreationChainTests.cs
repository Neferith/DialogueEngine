using DungeonCrawler.Characters.Creation;
using FluentAssertions;
using Xunit;

namespace DungeonCrawler.Characters.Tests;

public class CreationChainTests
{
    // ── Gender → Size ─────────────────────────────────────────────────────────

    [Fact]
    public void Male_HasAllSizes()
    {
        var sizes = CharacterGender.Male.AvailableSizes();
        sizes.Should().HaveCount(Enum.GetValues<CharacterSize>().Length);
    }

    [Fact]
    public void Female_ExcludesLargeAndGiant()
    {
        var sizes = CharacterGender.Female.AvailableSizes();
        sizes.Should().NotContain(CharacterSize.Large);
        sizes.Should().NotContain(CharacterSize.Giant);
    }

    // ── Size → Weight ─────────────────────────────────────────────────────────

    [Fact]
    public void Dwarf_OnlyLightAndAverage()
    {
        var weights = CharacterSize.Dwarf.AvailableWeights();
        weights.Should().BeEquivalentTo([CharacterWeight.Light, CharacterWeight.Average]);
    }

    [Fact]
    public void Giant_OnlyVeryHeavyAndExtreme()
    {
        var weights = CharacterSize.Giant.AvailableWeights();
        weights.Should().BeEquivalentTo([CharacterWeight.VeryHeavy, CharacterWeight.Extreme]);
    }

    // ── Weight → Sensitivity ──────────────────────────────────────────────────

    [Fact]
    public void Light_OnlyHypersensitiveAndHigh()
    {
        var sensitivities = CharacterWeight.Light.AvailableSensitivities();
        sensitivities.Should().BeEquivalentTo(
            [CharacterSensitivity.Hypersensitive, CharacterSensitivity.High]);
    }

    [Fact]
    public void Extreme_OnlyLowAndInsensitive()
    {
        var sensitivities = CharacterWeight.Extreme.AvailableSensitivities();
        sensitivities.Should().BeEquivalentTo(
            [CharacterSensitivity.Low, CharacterSensitivity.Insensitive]);
    }

    // ── Modifiers ─────────────────────────────────────────────────────────────

    [Fact]
    public void Giant_HasHighMusculatureModifier()
    {
        CharacterSize.Giant.Modifier().Musculature.Should().Be(+2);
        CharacterSize.Giant.Modifier().Flexibility.Should().Be(-4);
    }

    [Fact]
    public void Hypersensitive_HasPositiveBrainModifier()
    {
        CharacterSensitivity.Hypersensitive.Modifier().Brain.Should().Be(+2);
        CharacterSensitivity.Hypersensitive.Modifier().Vitality.Should().Be(-2);
    }
}