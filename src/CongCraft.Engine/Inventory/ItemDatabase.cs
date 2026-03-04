namespace CongCraft.Engine.Inventory;

/// <summary>
/// Central registry of all item definitions. Simple dictionary lookup.
/// </summary>
public static class ItemDatabase
{
    private static readonly Dictionary<string, ItemData> Items = new();

    static ItemDatabase()
    {
        Register(new ItemData
        {
            Id = "rusty_sword", Name = "Rusty Sword", Type = ItemType.Weapon,
            Slot = ItemSlot.MainHand, Weight = 3f,
            AttackBonus = 5f, IconR = 0.6f, IconG = 0.4f, IconB = 0.3f
        });
        Register(new ItemData
        {
            Id = "iron_sword", Name = "Iron Sword", Type = ItemType.Weapon,
            Slot = ItemSlot.MainHand, Weight = 4f,
            AttackBonus = 10f, IconR = 0.7f, IconG = 0.7f, IconB = 0.75f
        });
        Register(new ItemData
        {
            Id = "dark_blade", Name = "Dark Blade", Type = ItemType.Weapon,
            Slot = ItemSlot.MainHand, Weight = 5f,
            AttackBonus = 18f, IconR = 0.3f, IconG = 0.1f, IconB = 0.4f
        });
        Register(new ItemData
        {
            Id = "leather_helm", Name = "Leather Helm", Type = ItemType.Armor,
            Slot = ItemSlot.Head, Weight = 2f,
            DefenseBonus = 3f, IconR = 0.5f, IconG = 0.35f, IconB = 0.2f
        });
        Register(new ItemData
        {
            Id = "chain_mail", Name = "Chain Mail", Type = ItemType.Armor,
            Slot = ItemSlot.Chest, Weight = 8f,
            DefenseBonus = 8f, SpeedBonus = -1f,
            IconR = 0.55f, IconG = 0.55f, IconB = 0.6f
        });
        Register(new ItemData
        {
            Id = "iron_greaves", Name = "Iron Greaves", Type = ItemType.Armor,
            Slot = ItemSlot.Legs, Weight = 5f,
            DefenseBonus = 5f, SpeedBonus = -0.5f,
            IconR = 0.5f, IconG = 0.5f, IconB = 0.55f
        });
        Register(new ItemData
        {
            Id = "health_potion", Name = "Health Potion", Type = ItemType.Consumable,
            Slot = ItemSlot.None, Weight = 0.5f,
            HealthBonus = 30f, IconR = 0.8f, IconG = 0.15f, IconB = 0.15f
        });
        Register(new ItemData
        {
            Id = "wolf_pelt", Name = "Wolf Pelt", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 2f,
            IconR = 0.4f, IconG = 0.35f, IconB = 0.3f
        });
        Register(new ItemData
        {
            Id = "gold_coin", Name = "Gold Coin", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 0.1f,
            IconR = 0.9f, IconG = 0.8f, IconB = 0.2f
        });
        Register(new ItemData
        {
            Id = "iron_ore", Name = "Iron Ore", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 3f,
            IconR = 0.45f, IconG = 0.45f, IconB = 0.5f
        });
        Register(new ItemData
        {
            Id = "herb", Name = "Herb", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 0.2f,
            IconR = 0.2f, IconG = 0.7f, IconB = 0.2f
        });
        Register(new ItemData
        {
            Id = "leather", Name = "Leather", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 1f,
            IconR = 0.55f, IconG = 0.35f, IconB = 0.15f
        });
        Register(new ItemData
        {
            Id = "wood", Name = "Wood", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 2f,
            IconR = 0.5f, IconG = 0.35f, IconB = 0.2f
        });
        Register(new ItemData
        {
            Id = "cloth", Name = "Cloth", Type = ItemType.Material,
            Slot = ItemSlot.None, Weight = 0.5f,
            IconR = 0.75f, IconG = 0.7f, IconB = 0.6f
        });
        Register(new ItemData
        {
            Id = "strength_elixir", Name = "Strength Elixir", Type = ItemType.Consumable,
            Slot = ItemSlot.None, Weight = 0.5f,
            AttackBonus = 10f, IconR = 0.8f, IconG = 0.4f, IconB = 0.1f
        });
    }

    private static void Register(ItemData item) => Items[item.Id] = item;

    public static ItemData? Get(string id) => Items.GetValueOrDefault(id);

    public static IReadOnlyDictionary<string, ItemData> All => Items;
}
