using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class NpcMeshBuilderTests
{
    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_HasVertices(string npcType)
    {
        var data = npcType switch
        {
            "blacksmith" => NpcMeshBuilder.GenerateBlacksmith(),
            "elder" => NpcMeshBuilder.GenerateElder(),
            "merchant" => NpcMeshBuilder.GenerateMerchant(),
            _ => throw new ArgumentException()
        };
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_VertexCountDivisibleByStride(string npcType)
    {
        var data = npcType switch
        {
            "blacksmith" => NpcMeshBuilder.GenerateBlacksmith(),
            "elder" => NpcMeshBuilder.GenerateElder(),
            "merchant" => NpcMeshBuilder.GenerateMerchant(),
            _ => throw new ArgumentException()
        };
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Theory]
    [InlineData("blacksmith")]
    [InlineData("elder")]
    [InlineData("merchant")]
    public void GenerateData_IndicesWithinBounds(string npcType)
    {
        var data = npcType switch
        {
            "blacksmith" => NpcMeshBuilder.GenerateBlacksmith(),
            "elder" => NpcMeshBuilder.GenerateElder(),
            "merchant" => NpcMeshBuilder.GenerateMerchant(),
            _ => throw new ArgumentException()
        };
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
    }
}
