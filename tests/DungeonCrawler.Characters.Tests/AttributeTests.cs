using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using FluentAssertions;
using Xunit;

namespace DungeonCrawler.Characters.Tests;

public class AttributeTests
{
    // ── AttributesModifier ────────────────────────────────────────────────────

    [Fact]
    public void Modifier_Plus_StacksCorrectly()
    {
        var a = new AttributesModifier(Musculature: +2, Flexibility: +1);
        var b = new AttributesModifier(Musculature: -1, Brain: +3);

        var result = a + b;

        result.Musculature.Should().Be(1);
        result.Flexibility.Should().Be(1);
        result.Brain.Should().Be(3);
        result.Vitality.Should().Be(0);
    }

    [Fact]
    public void Modifier_Zero_IsNeutralElement()
    {
        var mod = new AttributesModifier(Musculature: +3, Vitality: -2);
        var result = mod + AttributesModifier.Zero;
        result.Should().Be(mod);
    }

    // ── Attributes.ApplyFixed ─────────────────────────────────────────────────

    [Fact]
    public void ApplyFixed_AddsModifierExactly()
    {
        var attrs = CharacterAttributes.Empty;
        var mod = new AttributesModifier(Musculature: +2, Brain: -1);

        var result = attrs.ApplyFixed(mod);

        result.Musculature.Current().Should().Be(2);
        result.Brain.Current().Should().Be(-1);
        result.Flexibility.Current().Should().Be(0);
        result.Vitality.Current().Should().Be(0);
    }

    [Fact]
    public void ApplyFixed_Multiple_Accumulates()
    {
        var attrs = CharacterAttributes.Empty
            .ApplyFixed(new AttributesModifier(Musculature: +1, Vitality: +1))
            .ApplyFixed(new AttributesModifier(Musculature: +1, Brain: +2));

        attrs.Musculature.Current().Should().Be(2);
        attrs.Vitality.Current().Should().Be(1);
        attrs.Brain.Current().Should().Be(2);
    }

    // ── Attributes.ApplyModifier (aléatoire) ──────────────────────────────────

    [Fact]
    public void ApplyModifier_PositiveValue_StaysInRange()
    {
        var attrs = CharacterAttributes.Empty;
        var mod = new AttributesModifier(Musculature: +3);

        for (int i = 0; i < 100; i++)
        {
            var result = attrs.ApplyModifier(mod);
            result.Musculature.Current().Should().BeInRange(0, 3);
        }
    }

    [Fact]
    public void ApplyModifier_NegativeValue_StaysInRange()
    {
        var attrs = CharacterAttributes.Empty;
        var mod = new AttributesModifier(Flexibility: -2);

        for (int i = 0; i < 100; i++)
        {
            var result = attrs.ApplyModifier(mod);
            result.Flexibility.Current().Should().BeInRange(-2, 0);
        }
    }

    [Fact]
    public void ApplyModifier_ZeroValue_AlwaysZero()
    {
        var attrs = CharacterAttributes.Empty;
        var mod = new AttributesModifier(Brain: 0);

        for (int i = 0; i < 50; i++)
        {
            var result = attrs.ApplyModifier(mod);
            result.Brain.Current().Should().Be(0);
        }
    }

    // ── Attribute clamping ────────────────────────────────────────────────────

    [Fact]
    public void Attribute_Current_ClampsToMax()
    {
        var attr = new CharacterAttribute(Permanent: 15, Min: -10, Max: 10);
        attr.Current().Should().Be(10);
    }

    [Fact]
    public void Attribute_Current_ClampsToMin()
    {
        var attr = new CharacterAttribute(Permanent: -15, Min: -10, Max: 10);
        attr.Current().Should().Be(-10);
    }
}