using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.Persistence;
using FluentAssertions;
using Xunit;

public class CharacterMapperTests
{
    private static Character CreateTestCharacter()
    {
        var attrs = new CharacterAttributes(
            new CharacterAttribute(8),
            new CharacterAttribute(6),
            new CharacterAttribute(7),
            new CharacterAttribute(9));

        var desc = new CharacterDescription(
            Name: new CharacterName("Aldric", "Vorn"),
            Age: 30,
            Gender: CharacterGender.Male,
            Size: CharacterSize.Medium,
            Weight: CharacterWeight.Average,
            Sensitivity: CharacterSensitivity.Normal,
            Background: null);

        return Character.Create(desc, attrs);
    }

    [Fact]
    public void RoundTrip_PreservesId()
    {
        var original = CreateTestCharacter();
        var save = CharacterMapper.ToSaveData(original);
        var restored = CharacterMapper.FromSaveData(save);

        restored.Id.Should().Be(original.Id); // FAIL actuellement
    }

    [Fact]
    public void RoundTrip_PreservesAttributes()
    {
        var original = CreateTestCharacter();
        var restored = CharacterMapper.FromSaveData(CharacterMapper.ToSaveData(original));

        restored.Attributes.Musculature.Permanent.Should().Be(original.Attributes.Musculature.Permanent);
        restored.Attributes.Flexibility.Permanent.Should().Be(original.Attributes.Flexibility.Permanent);
        restored.Attributes.Brain.Permanent.Should().Be(original.Attributes.Brain.Permanent);
        restored.Attributes.Vitality.Permanent.Should().Be(original.Attributes.Vitality.Permanent);
    }

    [Fact]
    public void RoundTrip_PreservesCurrentHp()
    {
        var original = CreateTestCharacter().WithDamage(5);
        var restored = CharacterMapper.FromSaveData(CharacterMapper.ToSaveData(original));

        restored.State.CurrentHp.Should().Be(original.State.CurrentHp);
    }

    [Fact]
    public void RoundTrip_PreservesInventory()
    {
        var original = CreateTestCharacter();
        original.Inventory.Add("rusty_key", 1);
        original.Inventory.Add("healing_herb", 3);

        var restored = CharacterMapper.FromSaveData(CharacterMapper.ToSaveData(original));

        restored.Inventory.GetQuantity("rusty_key").Should().Be(1);
        restored.Inventory.GetQuantity("healing_herb").Should().Be(3);
    }

    [Fact]
    public void RoundTrip_PreservesFactionId()
    {
        var attrs = new CharacterAttributes(
            new CharacterAttribute(8), new CharacterAttribute(6),
            new CharacterAttribute(7), new CharacterAttribute(9));
        var desc = new CharacterDescription(
            new CharacterName("Aldric", "Vorn"), 30,
            CharacterGender.Male, CharacterSize.Medium,
            CharacterWeight.Average, CharacterSensitivity.Normal, null);

        var original = Character.Create(desc, attrs, factionId: "nostro_guards");
        var restored = CharacterMapper.FromSaveData(CharacterMapper.ToSaveData(original));

        restored.FactionId.Should().Be("nostro_guards");
    }
}