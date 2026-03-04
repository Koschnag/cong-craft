using CongCraft.Engine.Inventory;

namespace CongCraft.Engine.Tests.Inventory;

public class ItemDatabaseTests
{
    [Fact]
    public void Get_ReturnsKnownItem()
    {
        var sword = ItemDatabase.Get("rusty_sword");
        Assert.NotNull(sword);
        Assert.Equal("Rusty Sword", sword.Name);
        Assert.Equal(ItemType.Weapon, sword.Type);
        Assert.Equal(ItemSlot.MainHand, sword.Slot);
    }

    [Fact]
    public void Get_ReturnsNullForUnknown()
    {
        Assert.Null(ItemDatabase.Get("nonexistent_item"));
    }

    [Fact]
    public void All_ContainsExpectedItems()
    {
        var all = ItemDatabase.All;
        Assert.True(all.Count >= 9, "Should have at least 9 items defined");
        Assert.Contains("iron_sword", all.Keys);
        Assert.Contains("health_potion", all.Keys);
        Assert.Contains("gold_coin", all.Keys);
        Assert.Contains("chain_mail", all.Keys);
    }

    [Fact]
    public void AllWeapons_HaveMainHandSlot()
    {
        foreach (var (_, item) in ItemDatabase.All)
        {
            if (item.Type == ItemType.Weapon)
                Assert.Equal(ItemSlot.MainHand, item.Slot);
        }
    }

    [Fact]
    public void HealthPotion_HasHealthBonus()
    {
        var potion = ItemDatabase.Get("health_potion");
        Assert.NotNull(potion);
        Assert.Equal(ItemType.Consumable, potion.Type);
        Assert.True(potion.HealthBonus > 0, "Health potion should heal");
    }
}
