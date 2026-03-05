using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class EnemyMeshBuilderTests
{
    [Fact]
    public void GenerateData_HasVertices()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.True(data.Vertices.Length > 0, "Enemy mesh should have vertices");
    }

    [Fact]
    public void GenerateData_HasIndices()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.True(data.Indices.Length > 0, "Enemy mesh should have indices");
    }

    [Fact]
    public void GenerateData_VertexCountDivisibleByStride()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_HasSubstantialGeometry()
    {
        var data = EnemyMeshBuilder.GenerateData();
        // Smooth mesh with capsules, spheres, and cylinders has significantly
        // more vertices than the old box-based approach
        Assert.True(data.VertexCount > 100, "Smooth enemy mesh should have substantial geometry");
    }

    [Fact]
    public void GenerateData_IndicesWithinBounds()
    {
        var data = EnemyMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
        }
    }
}
