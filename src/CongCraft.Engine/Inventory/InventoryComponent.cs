using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Inventory;

/// <summary>
/// Holds an entity's item inventory with a capacity limit.
/// </summary>
public sealed class InventoryComponent : IComponent
{
    public List<ItemStack> Items { get; } = new();
    public int Capacity { get; set; } = 20;
    public bool IsOpen { get; set; }
    public int SelectedSlot { get; set; }

    public bool IsFull => Items.Count >= Capacity;

    /// <summary>
    /// Adds an item. Returns true if successfully added, stacks if possible.
    /// </summary>
    public bool TryAdd(ItemData item, int quantity = 1)
    {
        // Try stacking with existing
        var existing = Items.Find(s => s.Item.Id == item.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
            return true;
        }

        if (IsFull) return false;

        Items.Add(new ItemStack { Item = item, Quantity = quantity });
        return true;
    }

    /// <summary>
    /// Removes quantity of an item. Returns true if enough was available.
    /// </summary>
    public bool TryRemove(string itemId, int quantity = 1)
    {
        var stack = Items.Find(s => s.Item.Id == itemId);
        if (stack == null || stack.Quantity < quantity) return false;

        stack.Quantity -= quantity;
        if (stack.Quantity <= 0)
            Items.Remove(stack);
        return true;
    }

    public ItemStack? GetAt(int index) =>
        index >= 0 && index < Items.Count ? Items[index] : null;

    public int CountOf(string itemId) =>
        Items.Find(s => s.Item.Id == itemId)?.Quantity ?? 0;
}
