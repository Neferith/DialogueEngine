using DungeonCrawler.Core;
using FluentAssertions;
using Xunit;

namespace DungeonCrawler.Core.Tests;

public class ItemRegistryTests
{
    private static ItemDefinition MakeItem(string id, ItemType? type = null) =>
        new()
        {
            Id = id,
            Title = $"Titre {id}",
            Description = $"Description {id}",
            Type = type ?? new ItemType.Other(),
            StackRules = new StackRules()
        };

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public void Register_NewItem_AddsToRegistry()
    {
        var registry = new ItemRegistry();
        registry.Register(MakeItem("sword"));

        registry.Contains("sword").Should().BeTrue();
    }

    [Fact]
    public void Register_DuplicateId_Throws()
    {
        var registry = new ItemRegistry();
        registry.Register(MakeItem("sword"));

        var act = () => registry.Register(MakeItem("sword"));
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*sword*");
    }

    // ── Get ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Get_ExistingItem_ReturnsDefinition()
    {
        var registry = new ItemRegistry();
        var item = MakeItem("nostro_cross", new ItemType.Quest());
        registry.Register(item);

        var result = registry.Get("nostro_cross");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Titre nostro_cross");
        result.Type.Should().BeOfType<ItemType.Quest>();
    }

    [Fact]
    public void Get_UnknownItem_ReturnsNull()
    {
        var registry = new ItemRegistry();
        registry.Get("unknown").Should().BeNull();
    }

    // ── All ───────────────────────────────────────────────────────────────────

    [Fact]
    public void All_ReturnsAllRegisteredItems()
    {
        var registry = new ItemRegistry();
        registry.Register(MakeItem("sword"));
        registry.Register(MakeItem("shield"));
        registry.Register(MakeItem("potion"));

        registry.All.Should().HaveCount(3);
    }

    // ── StackRules ────────────────────────────────────────────────────────────

    [Fact]
    public void StackRules_NonStackable_HasMaxOne()
    {
        var rules = new StackRules(Stackable: false, Max: 1);
        rules.Stackable.Should().BeFalse();
        rules.Max.Should().Be(1);
    }

    [Fact]
    public void StackRules_Stackable_CanHaveHigherMax()
    {
        var rules = new StackRules(Stackable: true, Max: 99);
        rules.Stackable.Should().BeTrue();
        rules.Max.Should().Be(99);
    }

    // ── ItemType ──────────────────────────────────────────────────────────────

    [Fact]
    public void ItemType_Quest_IsDistinctFromOther()
    {
        ItemType quest = new ItemType.Quest();
        ItemType other = new ItemType.Other();

        quest.Should().NotBe(other);
        quest.Should().BeOfType<ItemType.Quest>();
    }

    [Fact]
    public void ItemType_Equipment_IsDistinctFromQuest()
    {
        ItemType equipment = new ItemType.Equipment();
        ItemType quest = new ItemType.Quest();

        equipment.Should().NotBe(quest);
    }
}