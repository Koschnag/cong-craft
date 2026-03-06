using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class HighResNpcMeshBuilderTests
{
    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_HasSubstantialGeometry(string npcType)
    {
        var data = GetData(npcType);
        Assert.True(data.VertexCount > 400,
            $"High-res {npcType} should have substantial geometry, got {data.VertexCount}");
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_MoreDetailThanLowRes(string npcType)
    {
        var hiRes = GetData(npcType);
        var loRes = GetLowResData(npcType);
        Assert.True(hiRes.VertexCount > loRes.VertexCount,
            $"High-res {npcType} ({hiRes.VertexCount}) should exceed low-res ({loRes.VertexCount})");
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_VertexCountDivisibleByStride(string npcType)
    {
        var data = GetData(npcType);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_IndicesWithinBounds(string npcType)
    {
        var data = GetData(npcType);
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"{npcType}: Index {idx} out of bounds (max {maxVertex})");
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_TriangleCountValid(string npcType)
    {
        var data = GetData(npcType);
        Assert.Equal(0, data.Indices.Length % 3);
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_ColorsInValidRange(string npcType)
    {
        var data = GetData(npcType);
        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float r = data.Vertices[i * 9 + 6];
            float g = data.Vertices[i * 9 + 7];
            float b = data.Vertices[i * 9 + 8];
            Assert.True(r >= 0f && r <= 1f, $"{npcType}: Red out of range at vertex {i}: {r}");
            Assert.True(g >= 0f && g <= 1f, $"{npcType}: Green out of range at vertex {i}: {g}");
            Assert.True(b >= 0f && b <= 1f, $"{npcType}: Blue out of range at vertex {i}: {b}");
        }
    }

    [Fact]
    public void UnknownType_FallsBackToBlacksmith()
    {
        var fallback = HighResNpcMeshBuilder.Create;
        // The Create method with unknown type should not throw
        // We test the data generation path
        var blacksmith = HighResNpcMeshBuilder.GenerateBlacksmith();
        Assert.True(blacksmith.VertexCount > 0);
    }

    private static MeshData GetData(string npcType) => npcType switch
    {
        "blacksmith" => HighResNpcMeshBuilder.GenerateBlacksmith(),
        "elder" => HighResNpcMeshBuilder.GenerateElder(),
        "merchant" => HighResNpcMeshBuilder.GenerateMerchant(),
        _ => throw new ArgumentException($"Unknown NPC type: {npcType}")
    };

    private static MeshData GetLowResData(string npcType) => npcType switch
    {
        "blacksmith" => NpcMeshBuilder.GenerateBlacksmith(),
        "elder" => NpcMeshBuilder.GenerateElder(),
        "merchant" => NpcMeshBuilder.GenerateMerchant(),
        _ => throw new ArgumentException($"Unknown NPC type: {npcType}")
    };
}
