namespace CongCraft.Engine.Inventory;

/// <summary>
/// Defines an item type with its properties and stat modifiers.
/// </summary>
public sealed class ItemData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public ItemType Type { get; init; }
    public ItemSlot Slot { get; init; } = ItemSlot.None;
    public float Weight { get; init; } = 1f;

    // Stat modifiers (applied when equipped)
    public float AttackBonus { get; init; }
    public float DefenseBonus { get; init; }
    public float SpeedBonus { get; init; }
    public float HealthBonus { get; init; }

    // Visual: color for HUD icon
    public float IconR { get; init; } = 0.5f;
    public float IconG { get; init; } = 0.5f;
    public float IconB { get; init; } = 0.5f;
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Material,
    Quest
}

public enum ItemSlot
{
    None,
    MainHand,
    Head,
    Chest,
    Legs
}
