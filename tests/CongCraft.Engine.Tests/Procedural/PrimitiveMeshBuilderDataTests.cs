using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class PrimitiveMeshBuilderDataTests
{
    [Fact]
    public void CreateCapsuleData_ProducesVertices()
    {
        var data = PrimitiveMeshBuilder.CreateCapsuleData(0.3f, 1.8f, 12, 0.6f, 0.5f, 0.4f);
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Fact]
    public void CreateCapsuleData_VerticesMultipleOfNine()
    {
        var data = PrimitiveMeshBuilder.CreateCapsuleData(0.3f, 1.8f, 12, 0.6f, 0.5f, 0.4f);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void CreateCapsuleData_IndicesAreValid()
    {
        var data = PrimitiveMeshBuilder.CreateCapsuleData(0.3f, 1.8f, 8, 0.5f, 0.5f, 0.5f);
        uint maxIdx = (uint)(data.VertexCount - 1);
        foreach (var idx in data.Indices)
            Assert.InRange(idx, 0u, maxIdx);
    }

    [Fact]
    public void CreateCubeData_ProducesVertices()
    {
        var data = PrimitiveMeshBuilder.CreateCubeData(0.5f, 0.5f, 0.5f);
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Fact]
    public void CreateCubeData_VerticesMultipleOfNine()
    {
        var data = PrimitiveMeshBuilder.CreateCubeData(0.5f, 0.5f, 0.5f);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void CreateCubeData_Has24Vertices()
    {
        // 6 faces × 4 vertices each = 24
        var data = PrimitiveMeshBuilder.CreateCubeData(0.5f, 0.5f, 0.5f);
        Assert.Equal(24, data.VertexCount);
    }

    [Fact]
    public void CreateCubeData_Has36Indices()
    {
        // 6 faces × 2 triangles × 3 indices = 36
        var data = PrimitiveMeshBuilder.CreateCubeData(0.5f, 0.5f, 0.5f);
        Assert.Equal(36, data.Indices.Length);
    }

    [Fact]
    public void CreateCubeData_ColorsApplied()
    {
        var data = PrimitiveMeshBuilder.CreateCubeData(0.8f, 0.2f, 0.1f);
        // Color at vertex 0: offset 6,7,8
        Assert.Equal(0.8f, data.Vertices[6], 3);
        Assert.Equal(0.2f, data.Vertices[7], 3);
        Assert.Equal(0.1f, data.Vertices[8], 3);
    }

    [Fact]
    public void CreateCubeData_IndicesAreValid()
    {
        var data = PrimitiveMeshBuilder.CreateCubeData(0.5f, 0.5f, 0.5f);
        uint maxIdx = (uint)(data.VertexCount - 1);
        foreach (var idx in data.Indices)
            Assert.InRange(idx, 0u, maxIdx);
    }
}
