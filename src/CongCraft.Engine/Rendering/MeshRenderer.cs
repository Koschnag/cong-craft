using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Component that links an entity to a renderable mesh and shader.
/// </summary>
public class MeshRendererComponent : IComponent
{
    public required Mesh Mesh { get; init; }
    public required Shader Shader { get; init; }
    public bool CastShadow { get; init; } = true;
}
