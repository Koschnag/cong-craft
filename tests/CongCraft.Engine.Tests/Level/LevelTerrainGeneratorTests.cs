using System.Numerics;
using CongCraft.Engine.Level;

namespace CongCraft.Engine.Tests.Level;

public class LevelTerrainGeneratorTests
{
    private static LevelTerrainGenerator CreateGenerator()
    {
        return new LevelTerrainGenerator(LevelOneData.Create());
    }

    [Fact]
    public void GetHeightAt_Origin_ReturnsFiniteValue()
    {
        var gen = CreateGenerator();
        float h = gen.GetHeightAt(0, 0);
        Assert.True(float.IsFinite(h), $"Height at origin should be finite, got {h}");
    }

    [Fact]
    public void GetHeightAt_DifferentPositions_VaryHeight()
    {
        var gen = CreateGenerator();
        float h1 = gen.GetHeightAt(0, 10);
        float h2 = gen.GetHeightAt(50, -50);
        // Two distant positions should generally have different heights
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void GetHeightAt_SamePosition_Deterministic()
    {
        var gen = CreateGenerator();
        float h1 = gen.GetHeightAt(15, 25);
        float h2 = gen.GetHeightAt(15, 25);
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void GenerateChunk_ReturnsValidData()
    {
        var gen = CreateGenerator();
        var chunk = gen.GenerateChunk(0, 0);
        Assert.NotNull(chunk);
        Assert.True(chunk.Heights.Length > 0);
        Assert.True(chunk.Normals.Length > 0);
        Assert.True(chunk.Positions.Length > 0);
    }

    [Fact]
    public void GenerateChunk_CorrectVertexCount()
    {
        var gen = CreateGenerator();
        int resolution = 64;
        var chunk = gen.GenerateChunk(0, 0, resolution);
        int expected = (resolution + 1) * (resolution + 1);
        Assert.Equal(expected, chunk.Heights.Length);
        Assert.Equal(expected, chunk.Normals.Length);
        Assert.Equal(expected, chunk.Positions.Length);
    }

    [Fact]
    public void GenerateChunk_NormalsAreUnitLength()
    {
        var gen = CreateGenerator();
        var chunk = gen.GenerateChunk(0, 0, 16);
        foreach (var normal in chunk.Normals)
        {
            float len = normal.Length();
            Assert.True(len > 0.5f && len < 1.5f,
                $"Normal should be roughly unit length, got {len:F3}");
        }
    }

    [Fact]
    public void GetPathInfluence_FarFromPaths_ReturnsZero()
    {
        var gen = CreateGenerator();
        // Far corner of the world — unlikely to have a path
        float influence = gen.GetPathInfluence(120, 120);
        Assert.True(influence >= 0f && influence <= 1f,
            $"Path influence should be [0,1], got {influence}");
    }

    [Fact]
    public void GetDominantZone_ReturnsValidType()
    {
        var gen = CreateGenerator();
        var (type, strength) = gen.GetDominantZone(0, 10);
        Assert.False(string.IsNullOrEmpty(type));
        Assert.True(strength >= 0f);
    }

    [Fact]
    public void GetPathInfluence_AlwaysNonNegative()
    {
        var gen = CreateGenerator();
        for (int x = -50; x <= 50; x += 25)
        for (int z = -50; z <= 50; z += 25)
        {
            float influence = gen.GetPathInfluence(x, z);
            Assert.True(influence >= 0f, $"Path influence at ({x},{z}) should be non-negative, got {influence}");
        }
    }
}
