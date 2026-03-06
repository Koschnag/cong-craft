using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class PlayerMeshBuilderTests
{
    [Fact]
    public void GenerateData_HasSubstantialGeometry()
    {
        var data = PlayerMeshBuilder.GenerateData();
        Assert.True(data.VertexCount > 100, "Player mesh should have substantial geometry");
    }

    [Fact]
    public void GenerateData_VertexCountDivisibleByStride()
    {
        var data = PlayerMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_IndicesWithinBounds()
    {
        var data = PlayerMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Player index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void GenerateData_HasReasonableColors()
    {
        var data = PlayerMeshBuilder.GenerateData();
        // Player should have warm brown/skin tones, not pure white or black
        int vertexCount = data.Vertices.Length / 9;
        float totalR = 0, totalG = 0;
        for (int i = 0; i < vertexCount; i++)
        {
            totalR += data.Vertices[i * 9 + 6];
            totalG += data.Vertices[i * 9 + 7];
        }
        float avgR = totalR / vertexCount;
        float avgG = totalG / vertexCount;
        Assert.True(avgR > 0.15f && avgR < 0.9f, $"Player color should be moderate (avg R={avgR:F2})");
        Assert.True(avgG > 0.1f && avgG < 0.8f, $"Player color should be moderate (avg G={avgG:F2})");
    }
}
