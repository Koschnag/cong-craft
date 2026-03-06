using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class BushMeshBuilderTests
{
    [Fact]
    public void GenerateScrub_HasVertices()
    {
        var data = BushMeshBuilder.GenerateScrub(42);
        Assert.True(data.VertexCount > 10, "Scrub bush should have vertices");
    }

    [Fact]
    public void GenerateScrub_VertexCountDivisibleByStride()
    {
        var data = BushMeshBuilder.GenerateScrub(42);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateScrub_IndicesWithinBounds()
    {
        var data = BushMeshBuilder.GenerateScrub(42);
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Scrub index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void GenerateBerry_HasVertices()
    {
        var data = BushMeshBuilder.GenerateBerry(42);
        Assert.True(data.VertexCount > 10, "Berry bush should have vertices");
    }

    [Fact]
    public void GenerateBerry_VertexCountDivisibleByStride()
    {
        var data = BushMeshBuilder.GenerateBerry(42);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateBerry_IndicesWithinBounds()
    {
        var data = BushMeshBuilder.GenerateBerry(42);
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Berry index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void GenerateGrassTuft_HasVertices()
    {
        var data = BushMeshBuilder.GenerateGrassTuft(42);
        Assert.True(data.VertexCount > 0, "Grass tuft should have vertices");
    }

    [Fact]
    public void GenerateGrassTuft_VertexCountDivisibleByStride()
    {
        var data = BushMeshBuilder.GenerateGrassTuft(42);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateGrassTuft_IndicesWithinBounds()
    {
        var data = BushMeshBuilder.GenerateGrassTuft(42);
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Grass tuft index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentMeshes()
    {
        var data1 = BushMeshBuilder.GenerateScrub(1);
        var data2 = BushMeshBuilder.GenerateScrub(2);
        // Different seeds should produce different vertex counts (random bush sizes)
        Assert.True(data1.VertexCount != data2.VertexCount || data1.Vertices[0] != data2.Vertices[0],
            "Different seeds should produce different bushes");
    }
}
