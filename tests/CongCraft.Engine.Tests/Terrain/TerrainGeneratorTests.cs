using CongCraft.Engine.Terrain;

namespace CongCraft.Engine.Tests.Terrain;

public class TerrainGeneratorTests
{
    [Fact]
    public void GenerateChunk_SameSeedSameCoords_Deterministic()
    {
        var gen1 = new TerrainGenerator(seed: 42);
        var gen2 = new TerrainGenerator(seed: 42);
        var h1 = gen1.GenerateChunk(0, 0, resolution: 16);
        var h2 = gen2.GenerateChunk(0, 0, resolution: 16);
        Assert.Equal(h1.Heights, h2.Heights);
    }

    [Fact]
    public void GenerateChunk_DifferentSeeds_DifferentHeights()
    {
        var gen1 = new TerrainGenerator(seed: 42);
        var gen2 = new TerrainGenerator(seed: 99);
        var h1 = gen1.GenerateChunk(0, 0, resolution: 16);
        var h2 = gen2.GenerateChunk(0, 0, resolution: 16);
        Assert.NotEqual(h1.Heights, h2.Heights);
    }

    [Fact]
    public void GenerateChunk_CorrectVertexCount()
    {
        var gen = new TerrainGenerator(seed: 42);
        var data = gen.GenerateChunk(0, 0, resolution: 32);
        Assert.Equal(33 * 33, data.Heights.Length); // (resolution + 1)^2
    }

    [Fact]
    public void GenerateChunk_NormalsAreUnitLength()
    {
        var gen = new TerrainGenerator(seed: 42);
        var data = gen.GenerateChunk(0, 0, resolution: 16);
        foreach (var normal in data.Normals)
        {
            Assert.InRange(normal.Length(), 0.98f, 1.02f);
        }
    }

    [Fact]
    public void GenerateChunk_NormalsPointUpward()
    {
        var gen = new TerrainGenerator(seed: 42);
        var data = gen.GenerateChunk(0, 0, resolution: 16);
        // Most normals should have positive Y (pointing up)
        int upwardCount = data.Normals.Count(n => n.Y > 0);
        Assert.True(upwardCount > data.Normals.Length * 0.9);
    }

    [Fact]
    public void BuildVertices_CorrectLength()
    {
        var gen = new TerrainGenerator(seed: 42);
        var data = gen.GenerateChunk(0, 0, resolution: 16);
        var verts = data.BuildVertices();
        // 8 floats per vertex (pos3 + normal3 + texcoord2)
        Assert.Equal(17 * 17 * 8, verts.Length);
    }

    [Fact]
    public void BuildIndices_CorrectLength()
    {
        var gen = new TerrainGenerator(seed: 42);
        var data = gen.GenerateChunk(0, 0, resolution: 16);
        var indices = data.BuildIndices();
        // 6 indices per quad, resolution * resolution quads
        Assert.Equal(16 * 16 * 6, indices.Length);
    }

    [Fact]
    public void GetHeightAt_Deterministic()
    {
        var gen = new TerrainGenerator(seed: 42);
        float h1 = gen.GetHeightAt(10f, 20f);
        float h2 = gen.GetHeightAt(10f, 20f);
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void GetHeightAt_DifferentPositions_DifferentHeights()
    {
        var gen = new TerrainGenerator(seed: 42);
        float h1 = gen.GetHeightAt(0f, 0f);
        float h2 = gen.GetHeightAt(100f, 100f);
        // Different positions should generally produce different heights
        Assert.NotEqual(h1, h2);
    }
}
