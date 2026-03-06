using CongCraft.Engine.Inventory;
using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class LootMeshBuilderTests
{
    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Material)]
    public void LootMesh_HasVertices(ItemType type)
    {
        var data = type switch
        {
            ItemType.Weapon => LootMeshBuilder.GenerateWeaponDrop(0.7f, 0.7f, 0.75f),
            ItemType.Armor => LootMeshBuilder.GenerateArmorDrop(0.5f, 0.45f, 0.4f),
            ItemType.Consumable => LootMeshBuilder.GeneratePotionDrop(0.8f, 0.2f, 0.2f),
            _ => LootMeshBuilder.GenerateMaterialDrop(0.6f, 0.5f, 0.3f),
        };
        Assert.True(data.VertexCount > 10, $"{type} loot should have vertices");
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Material)]
    public void LootMesh_VertexCountDivisibleByStride(ItemType type)
    {
        var data = type switch
        {
            ItemType.Weapon => LootMeshBuilder.GenerateWeaponDrop(0.7f, 0.7f, 0.75f),
            ItemType.Armor => LootMeshBuilder.GenerateArmorDrop(0.5f, 0.45f, 0.4f),
            ItemType.Consumable => LootMeshBuilder.GeneratePotionDrop(0.8f, 0.2f, 0.2f),
            _ => LootMeshBuilder.GenerateMaterialDrop(0.6f, 0.5f, 0.3f),
        };
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Material)]
    public void LootMesh_IndicesWithinBounds(ItemType type)
    {
        var data = type switch
        {
            ItemType.Weapon => LootMeshBuilder.GenerateWeaponDrop(0.7f, 0.7f, 0.75f),
            ItemType.Armor => LootMeshBuilder.GenerateArmorDrop(0.5f, 0.45f, 0.4f),
            ItemType.Consumable => LootMeshBuilder.GeneratePotionDrop(0.8f, 0.2f, 0.2f),
            _ => LootMeshBuilder.GenerateMaterialDrop(0.6f, 0.5f, 0.3f),
        };
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Loot index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void LootMesh_ColorsWithinRange()
    {
        var data = LootMeshBuilder.GeneratePotionDrop(0.8f, 0.2f, 0.2f);
        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float r = data.Vertices[i * 9 + 6];
            float g = data.Vertices[i * 9 + 7];
            float b = data.Vertices[i * 9 + 8];
            Assert.InRange(r, 0f, 1f);
            Assert.InRange(g, 0f, 1f);
            Assert.InRange(b, 0f, 1f);
        }
    }
}
