using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class SwordMeshBuilderTests
{
    [Fact]
    public void GenerateData_HasVertices()
    {
        var data = SwordMeshBuilder.GenerateData();
        Assert.True(data.Vertices.Length > 0, "Sword should have vertices");
    }

    [Fact]
    public void GenerateData_HasIndices()
    {
        var data = SwordMeshBuilder.GenerateData();
        Assert.True(data.Indices.Length > 0, "Sword should have indices");
    }

    [Fact]
    public void GenerateData_VertexCountDivisibleByStride()
    {
        var data = SwordMeshBuilder.GenerateData();
        // 9 floats per vertex: position(3) + normal(3) + color(3)
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_IndicesDivisibleByThree()
    {
        var data = SwordMeshBuilder.GenerateData();
        Assert.Equal(0, data.Indices.Length % 3);
    }

    [Fact]
    public void GenerateData_IndicesWithinBounds()
    {
        var data = SwordMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
        }
    }
}
