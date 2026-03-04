using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class RockMeshBuilderTests
{
    [Fact]
    public void GenerateData_ProducesVertices()
    {
        var data = RockMeshBuilder.GenerateData();
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Fact]
    public void GenerateData_DeterministicWithSameSeed()
    {
        var d1 = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Seed: 42));
        var d2 = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Seed: 42));
        Assert.Equal(d1.Vertices, d2.Vertices);
    }

    [Fact]
    public void GenerateData_DifferentSeeds_DifferentVertices()
    {
        var d1 = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Seed: 1));
        var d2 = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Seed: 999));
        Assert.NotEqual(d1.Vertices, d2.Vertices);
    }

    [Fact]
    public void GenerateData_MoreSubdivisions_MoreVertices()
    {
        var low = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Subdivisions: 1));
        var high = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Subdivisions: 3));
        Assert.True(high.VertexCount > low.VertexCount);
    }

    [Fact]
    public void GenerateData_IndicesAreValid()
    {
        var data = RockMeshBuilder.GenerateData();
        uint maxIndex = (uint)(data.VertexCount - 1);
        foreach (var idx in data.Indices)
        {
            Assert.InRange(idx, 0u, maxIndex);
        }
    }
}
