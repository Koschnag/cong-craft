namespace CongCraft.Engine.Inventory;

/// <summary>
/// A stack of items in an inventory slot.
/// </summary>
public sealed class ItemStack
{
    public required ItemData Item { get; init; }
    public int Quantity { get; set; } = 1;
}
