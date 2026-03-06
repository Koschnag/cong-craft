using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class HighResEnemyMeshBuilderTests
{
    [Fact]
    public void GenerateData_DarkKnight_HasSubstantialGeometry()
    {
        var data = HighResEnemyMeshBuilder.GenerateData();
        Assert.True(data.VertexCount > 500, $"High-res enemy should have substantial geometry, got {data.VertexCount}");
    }

    [Fact]
    public void GenerateData_DarkKnight_MoreDetailThanLowRes()
    {
        var hiRes = HighResEnemyMeshBuilder.GenerateData();
        var loRes = EnemyMeshBuilder.GenerateData();
        Assert.True(hiRes.VertexCount > loRes.VertexCount,
            $"High-res ({hiRes.VertexCount}) should have more vertices than low-res ({loRes.VertexCount})");
    }

    [Fact]
    public void GenerateData_DarkKnight_VertexStrideValid()
    {
        var data = HighResEnemyMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_DarkKnight_IndicesWithinBounds()
    {
        var data = HighResEnemyMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void GenerateSkeletonData_HasSubstantialGeometry()
    {
        var data = HighResEnemyMeshBuilder.GenerateSkeletonData();
        Assert.True(data.VertexCount > 400, $"High-res skeleton should have substantial geometry, got {data.VertexCount}");
    }

    [Fact]
    public void GenerateSkeletonData_MoreDetailThanLowRes()
    {
        var hiRes = HighResEnemyMeshBuilder.GenerateSkeletonData();
        var loRes = EnemyMeshBuilder.GenerateSkeletonData();
        Assert.True(hiRes.VertexCount > loRes.VertexCount,
            $"High-res skeleton ({hiRes.VertexCount}) should have more vertices than low-res ({loRes.VertexCount})");
    }

    [Fact]
    public void GenerateSkeletonData_VertexStrideValid()
    {
        var data = HighResEnemyMeshBuilder.GenerateSkeletonData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateSkeletonData_IndicesWithinBounds()
    {
        var data = HighResEnemyMeshBuilder.GenerateSkeletonData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Skeleton index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void GenerateWolfData_HasGeometry()
    {
        var data = HighResEnemyMeshBuilder.GenerateWolfData();
        Assert.True(data.VertexCount > 200, $"Wolf should have geometry, got {data.VertexCount}");
        Assert.Equal(0, data.Vertices.Length % 9);
        Assert.Equal(0, data.Indices.Length % 3);
    }

    [Fact]
    public void GenerateWolfData_IndicesWithinBounds()
    {
        var data = HighResEnemyMeshBuilder.GenerateWolfData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Wolf index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void GenerateTrollData_HasGeometry()
    {
        var data = HighResEnemyMeshBuilder.GenerateTrollData();
        Assert.True(data.VertexCount > 300, $"Troll should have geometry, got {data.VertexCount}");
        Assert.Equal(0, data.Vertices.Length % 9);
        Assert.Equal(0, data.Indices.Length % 3);
    }

    [Fact]
    public void GenerateTrollData_IndicesWithinBounds()
    {
        var data = HighResEnemyMeshBuilder.GenerateTrollData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Troll index {idx} out of bounds (max {maxVertex})");
    }

    [Theory]
    [InlineData("darkKnight")]
    [InlineData("skeleton")]
    [InlineData("wolf")]
    [InlineData("troll")]
    public void AllVariants_ColorsInValidRange(string variant)
    {
        var data = variant switch
        {
            "darkKnight" => HighResEnemyMeshBuilder.GenerateData(),
            "skeleton" => HighResEnemyMeshBuilder.GenerateSkeletonData(),
            "wolf" => HighResEnemyMeshBuilder.GenerateWolfData(),
            "troll" => HighResEnemyMeshBuilder.GenerateTrollData(),
            _ => throw new ArgumentException()
        };

        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float r = data.Vertices[i * 9 + 6];
            float g = data.Vertices[i * 9 + 7];
            float b = data.Vertices[i * 9 + 8];
            Assert.True(r >= 0f && r <= 1f, $"{variant}: Red out of range at vertex {i}: {r}");
            Assert.True(g >= 0f && g <= 1f, $"{variant}: Green out of range at vertex {i}: {g}");
            Assert.True(b >= 0f && b <= 1f, $"{variant}: Blue out of range at vertex {i}: {b}");
        }
    }
}
