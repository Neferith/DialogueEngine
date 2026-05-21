using DungeonCrawler.Core;
using FluentAssertions;
using Xunit;

namespace DungeonCrawler.Core.Tests;

public class InventoryTests
{
    // ── État initial ──────────────────────────────────────────────────────────

    [Fact]
    public void NewInventory_IsEmpty()
    {
        var inv = new Inventory();
        inv.IsEmpty.Should().BeTrue();
        inv.SlotCount.Should().Be(0);
    }

    [Fact]
    public void NewInventory_WithMaxSlots_IsNotFull()
    {
        var inv = new Inventory { MaxSlots = 10 };
        inv.IsFull.Should().BeFalse();
    }

    [Fact]
    public void NewInventory_WithoutMaxSlots_IsNeverFull()
    {
        var inv = new Inventory();
        for (int i = 0; i < 100; i++)
            inv.Add($"item_{i}");
        inv.IsFull.Should().BeFalse();
    }

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_NewItem_AppearsInInventory()
    {
        var inv = new Inventory();
        var result = inv.Add("sword");

        result.Should().BeTrue();
        inv.Contains("sword").Should().BeTrue();
        inv.GetQuantity("sword").Should().Be(1);
    }

    [Fact]
    public void Add_ExistingItem_StacksQuantity()
    {
        var inv = new Inventory();
        inv.Add("potion", 2);
        inv.Add("potion", 3);

        inv.GetQuantity("potion").Should().Be(5);
        inv.SlotCount.Should().Be(1);
    }

    [Fact]
    public void Add_MultipleItems_OccupiesMultipleSlots()
    {
        var inv = new Inventory();
        inv.Add("sword");
        inv.Add("shield");
        inv.Add("potion");

        inv.SlotCount.Should().Be(3);
    }

    [Fact]
    public void Add_WhenFull_RefusesNewItem()
    {
        var inv = new Inventory { MaxSlots = 2 };
        inv.Add("sword");
        inv.Add("shield");

        inv.IsFull.Should().BeTrue();
        var result = inv.Add("potion");

        result.Should().BeFalse();
        inv.Contains("potion").Should().BeFalse();
        inv.SlotCount.Should().Be(2);
    }

    [Fact]
    public void Add_WhenFull_AllowsStackingExistingItem()
    {
        var inv = new Inventory { MaxSlots = 2 };
        inv.Add("sword");
        inv.Add("potion", 1);

        var result = inv.Add("potion", 2);

        result.Should().BeTrue();
        inv.GetQuantity("potion").Should().Be(3);
        inv.SlotCount.Should().Be(2);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public void Remove_ReducesQuantity()
    {
        var inv = new Inventory();
        inv.Add("potion", 5);
        inv.Remove("potion", 2);

        inv.GetQuantity("potion").Should().Be(3);
    }

    [Fact]
    public void Remove_LastQuantity_RemovesSlot()
    {
        var inv = new Inventory();
        inv.Add("key", 1);
        inv.Remove("key", 1);

        inv.Contains("key").Should().BeFalse();
        inv.SlotCount.Should().Be(0);
    }

    [Fact]
    public void Remove_MoreThanOwned_RemovesSlot()
    {
        var inv = new Inventory();
        inv.Add("arrow", 3);
        inv.Remove("arrow", 10);

        inv.Contains("arrow").Should().BeFalse();
    }

    [Fact]
    public void Remove_UnknownItem_ReturnsFalse()
    {
        var inv = new Inventory();
        var result = inv.Remove("potion");

        result.Should().BeFalse();
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clear_EmptiesInventory()
    {
        var inv = new Inventory { MaxSlots = 10 };
        inv.Add("sword");
        inv.Add("shield");
        inv.Clear();

        inv.IsEmpty.Should().BeTrue();
        inv.MaxSlots.Should().Be(10);
    }
}