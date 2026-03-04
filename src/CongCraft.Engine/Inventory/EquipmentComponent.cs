using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Inventory;

/// <summary>
/// Tracks equipped items in named slots. Equipment modifies combat stats.
/// </summary>
public sealed class EquipmentComponent : IComponent
{
    public ItemStack? MainHand { get; set; }
    public ItemStack? Head { get; set; }
    public ItemStack? Chest { get; set; }
    public ItemStack? Legs { get; set; }

    public ItemStack? GetSlot(ItemSlot slot) => slot switch
    {
        ItemSlot.MainHand => MainHand,
        ItemSlot.Head => Head,
        ItemSlot.Chest => Chest,
        ItemSlot.Legs => Legs,
        _ => null
    };

    public void SetSlot(ItemSlot slot, ItemStack? item)
    {
        switch (slot)
        {
            case ItemSlot.MainHand: MainHand = item; break;
            case ItemSlot.Head: Head = item; break;
            case ItemSlot.Chest: Chest = item; break;
            case ItemSlot.Legs: Legs = item; break;
        }
    }

    /// <summary>
    /// Total stat bonuses from all equipped items.
    /// </summary>
    public float TotalAttackBonus =>
        (MainHand?.Item.AttackBonus ?? 0) + (Head?.Item.AttackBonus ?? 0) +
        (Chest?.Item.AttackBonus ?? 0) + (Legs?.Item.AttackBonus ?? 0);

    public float TotalDefenseBonus =>
        (MainHand?.Item.DefenseBonus ?? 0) + (Head?.Item.DefenseBonus ?? 0) +
        (Chest?.Item.DefenseBonus ?? 0) + (Legs?.Item.DefenseBonus ?? 0);

    public float TotalSpeedBonus =>
        (MainHand?.Item.SpeedBonus ?? 0) + (Head?.Item.SpeedBonus ?? 0) +
        (Chest?.Item.SpeedBonus ?? 0) + (Legs?.Item.SpeedBonus ?? 0);
}
