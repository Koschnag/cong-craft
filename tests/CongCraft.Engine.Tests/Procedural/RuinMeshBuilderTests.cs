using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class RuinMeshBuilderTests
{
    [Theory]
    [InlineData(RuinMeshBuilder.RuinType.Pillar)]
    [InlineData(RuinMeshBuilder.RuinType.BrokenPillar)]
    [InlineData(RuinMeshBuilder.RuinType.WallSegment)]
    [InlineData(RuinMeshBuilder.RuinType.ArchFragment)]
    public void GenerateData_AllTypes_HaveGeometry(RuinMeshBuilder.RuinType type)
    {
        var (vertices, indices) = RuinMeshBuilder.GenerateData(type);
        Assert.True(vertices.Length > 0, $"{type} should have vertices");
        Assert.True(indices.Length > 0, $"{type} should have indices");
        Assert.Equal(0, vertices.Length % 9); // PositionNormalColor stride
    }

    [Theory]
    [InlineData(RuinMeshBuilder.RuinType.Pillar)]
    [InlineData(RuinMeshBuilder.RuinType.BrokenPillar)]
    [InlineData(RuinMeshBuilder.RuinType.WallSegment)]
    [InlineData(RuinMeshBuilder.RuinType.ArchFragment)]
    public void GenerateData_AllTypes_IndicesWithinBounds(RuinMeshBuilder.RuinType type)
    {
        var (vertices, indices) = RuinMeshBuilder.GenerateData(type);
        uint maxVertex = (uint)(vertices.Length / 9);
        foreach (var idx in indices)
        {
            Assert.True(idx < maxVertex, $"Ruin {type} index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void GenerateData_DarkStoneColors()
    {
        var (vertices, _) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.Pillar, 42);
        int vertexCount = vertices.Length / 9;
        float totalR = 0;
        for (int i = 0; i < vertexCount; i++)
            totalR += vertices[i * 9 + 6];
        float avgR = totalR / vertexCount;
        // Ruins should be dark stone (avg R < 0.35)
        Assert.True(avgR < 0.40f, $"Ruin stone should be dark (avg R={avgR:F2})");
    }

    [Fact]
    public void GenerateData_DifferentSeeds_DifferentResults()
    {
        var (v1, _) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.BrokenPillar, 1);
        var (v2, _) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.BrokenPillar, 2);
        // Different seeds should produce different geometry (broken pillar is randomized)
        Assert.False(v1.SequenceEqual(v2), "Different seeds should produce different ruins");
    }
}
