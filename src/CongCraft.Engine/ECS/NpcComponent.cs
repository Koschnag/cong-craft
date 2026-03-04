using CongCraft.Engine.ECS;

namespace CongCraft.Engine.ECS;

/// <summary>
/// Marks an entity as an NPC with a dialogue tree and interaction state.
/// </summary>
public sealed class NpcComponent : IComponent
{
    public required string DialogueTreeId { get; init; }
    public required string Name { get; init; }
    public float InteractionRadius { get; set; } = 3f;
    public bool IsInteracting { get; set; }
}
