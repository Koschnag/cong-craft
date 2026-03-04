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
    public void GenerateData_SevenBoxes_CorrectVertexCount()
    {
        var data = EnemyMeshBuilder.GenerateData();
        // 7 boxes (body, head, 2 arms, 2 legs, helmet accent)
        // Each box = 6 faces * 4 vertices = 24 vertices * 9 floats
        int expectedVertices = 7 * 6 * 4 * 9;
        Assert.Equal(expectedVertices, data.Vertices.Length);
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
