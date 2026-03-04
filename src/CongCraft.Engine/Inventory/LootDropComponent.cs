using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Inventory;

/// <summary>
/// Marks an entity as a loot drop that can be picked up by the player.
/// </summary>
public sealed class LootDropComponent : IComponent
{
    public required ItemData Item { get; init; }
    public int Quantity { get; init; } = 1;
    public float PickupRadius { get; init; } = 2f;
    public float BobTimer { get; set; }
    public float SpawnY { get; set; }
}
