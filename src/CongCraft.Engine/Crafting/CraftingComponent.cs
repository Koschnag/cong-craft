using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Crafting;

/// <summary>
/// Marks an entity as a crafting station with a specific type.
/// </summary>
public sealed class CraftingComponent : IComponent
{
    public CraftingStationType StationType { get; init; }
    public float InteractionRadius { get; init; } = 3f;
}
