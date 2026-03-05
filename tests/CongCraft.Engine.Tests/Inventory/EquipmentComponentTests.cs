using CongCraft.Engine.Inventory;

namespace CongCraft.Engine.Tests.Inventory;

public class EquipmentComponentTests
{
    private static ItemStack MakeStack(ItemSlot slot, float attack = 0, float defense = 0, float speed = 0) =>
        new()
        {
            Item = new ItemData
            {
                Id = $"test_{slot}",
                Name = $"Test {slot}",
                Type = ItemType.Armor,
                Slot = slot,
                AttackBonus = attack,
                DefenseBonus = defense,
                SpeedBonus = speed
            }
        };

    [Fact]
    public void NewEquipment_AllSlotsEmpty()
    {
        var equip = new EquipmentComponent();
        Assert.Null(equip.MainHand);
        Assert.Null(equip.Head);
        Assert.Null(equip.Chest);
        Assert.Null(equip.Legs);
    }

    [Fact]
    public void SetSlot_EquipsItem()
    {
        var equip = new EquipmentComponent();
        var sword = MakeStack(ItemSlot.MainHand, attack: 10);
        equip.SetSlot(ItemSlot.MainHand, sword);
        Assert.Same(sword, equip.MainHand);
        Assert.Same(sword, equip.GetSlot(ItemSlot.MainHand));
    }

    [Fact]
    public void SetSlot_CanUnequip()
    {
        var equip = new EquipmentComponent();
        equip.SetSlot(ItemSlot.Head, MakeStack(ItemSlot.Head));
        equip.SetSlot(ItemSlot.Head, null);
        Assert.Null(equip.Head);
    }

    [Fact]
    public void TotalAttackBonus_SumsAllSlots()
    {
        var equip = new EquipmentComponent();
        equip.MainHand = MakeStack(ItemSlot.MainHand, attack: 10);
        equip.Head = MakeStack(ItemSlot.Head, attack: 2);
        Assert.Equal(12f, equip.TotalAttackBonus);
    }

    [Fact]
    public void TotalDefenseBonus_SumsAllSlots()
    {
        var equip = new EquipmentComponent();
        equip.Chest = MakeStack(ItemSlot.Chest, defense: 8);
        equip.Legs = MakeStack(ItemSlot.Legs, defense: 5);
        Assert.Equal(13f, equip.TotalDefenseBonus);
    }

    [Fact]
    public void TotalSpeedBonus_SumsAllSlots()
    {
        var equip = new EquipmentComponent();
        equip.Chest = MakeStack(ItemSlot.Chest, speed: -1f);
        equip.Legs = MakeStack(ItemSlot.Legs, speed: -0.5f);
        Assert.Equal(-1.5f, equip.TotalSpeedBonus);
    }

    [Fact]
    public void TotalBonuses_ZeroWhenEmpty()
    {
        var equip = new EquipmentComponent();
        Assert.Equal(0f, equip.TotalAttackBonus);
        Assert.Equal(0f, equip.TotalDefenseBonus);
        Assert.Equal(0f, equip.TotalSpeedBonus);
    }
}
