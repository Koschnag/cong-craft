namespace CongCraft.Engine.ECS;

/// <summary>
/// An entity is just an integer ID. All data lives in components.
/// </summary>
public readonly record struct Entity(int Id);
