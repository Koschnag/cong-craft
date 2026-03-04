using CongCraft.Engine.ECS;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Terrain;

/// <summary>
/// Component representing a single terrain chunk entity.
/// </summary>
public sealed class TerrainChunkComponent : IComponent
{
    public required int ChunkX { get; init; }
    public required int ChunkZ { get; init; }
    public required Mesh Mesh { get; init; }
    public HeightmapData? HeightmapData { get; init; }
}
