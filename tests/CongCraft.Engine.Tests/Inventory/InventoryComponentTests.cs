using CongCraft.Engine.Inventory;

namespace CongCraft.Engine.Tests.Inventory;

public class InventoryComponentTests
{
    private static ItemData MakeItem(string id = "test_item") => new()
    {
        Id = id, Name = "Test Item", Type = ItemType.Material
    };

    [Fact]
    public void NewInventory_IsEmpty()
    {
        var inv = new InventoryComponent();
        Assert.Empty(inv.Items);
        Assert.False(inv.IsFull);
    }

    [Fact]
    public void TryAdd_AddsItem()
    {
        var inv = new InventoryComponent();
        Assert.True(inv.TryAdd(MakeItem()));
        Assert.Single(inv.Items);
        Assert.Equal(1, inv.Items[0].Quantity);
    }

    [Fact]
    public void TryAdd_StacksExistingItem()
    {
        var inv = new InventoryComponent();
        var item = MakeItem();
        inv.TryAdd(item, 3);
        inv.TryAdd(item, 2);
        Assert.Single(inv.Items);
        Assert.Equal(5, inv.Items[0].Quantity);
    }

    [Fact]
    public void TryAdd_FailsWhenFull()
    {
        var inv = new InventoryComponent { Capacity = 2 };
        inv.TryAdd(MakeItem("a"));
        inv.TryAdd(MakeItem("b"));
        Assert.True(inv.IsFull);
        Assert.False(inv.TryAdd(MakeItem("c")));
    }

    [Fact]
    public void TryAdd_CanStillStackWhenFull()
    {
        var inv = new InventoryComponent { Capacity = 1 };
        var item = MakeItem();
        inv.TryAdd(item, 1);
        Assert.True(inv.IsFull);
        Assert.True(inv.TryAdd(item, 5)); // Stacks on existing
        Assert.Equal(6, inv.Items[0].Quantity);
    }

    [Fact]
    public void TryRemove_RemovesQuantity()
    {
        var inv = new InventoryComponent();
        inv.TryAdd(MakeItem(), 5);
        Assert.True(inv.TryRemove("test_item", 3));
        Assert.Equal(2, inv.Items[0].Quantity);
    }

    [Fact]
    public void TryRemove_RemovesStackWhenEmpty()
    {
        var inv = new InventoryComponent();
        inv.TryAdd(MakeItem(), 2);
        inv.TryRemove("test_item", 2);
        Assert.Empty(inv.Items);
    }

    [Fact]
    public void TryRemove_FailsIfNotEnough()
    {
        var inv = new InventoryComponent();
        inv.TryAdd(MakeItem(), 2);
        Assert.False(inv.TryRemove("test_item", 5));
        Assert.Equal(2, inv.Items[0].Quantity); // Unchanged
    }

    [Fact]
    public void TryRemove_FailsForMissingItem()
    {
        var inv = new InventoryComponent();
        Assert.False(inv.TryRemove("nonexistent"));
    }

    [Fact]
    public void CountOf_ReturnsCorrectCount()
    {
        var inv = new InventoryComponent();
        inv.TryAdd(MakeItem(), 7);
        Assert.Equal(7, inv.CountOf("test_item"));
        Assert.Equal(0, inv.CountOf("other"));
    }

    [Fact]
    public void GetAt_ReturnsCorrectSlot()
    {
        var inv = new InventoryComponent();
        inv.TryAdd(MakeItem("a"));
        inv.TryAdd(MakeItem("b"));
        Assert.Equal("a", inv.GetAt(0)?.Item.Id);
        Assert.Equal("b", inv.GetAt(1)?.Item.Id);
        Assert.Null(inv.GetAt(99));
        Assert.Null(inv.GetAt(-1));
    }
}
