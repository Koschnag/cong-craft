using CongCraft.Engine.Dungeon;

namespace CongCraft.Engine.Tests.Dungeon;

public class DungeonMeshBuilderTests
{
    private static DungeonLayout MakeSmallLayout()
    {
        var gen = new DungeonGenerator(20, 20, seed: 42);
        return gen.Generate(3);
    }

    [Fact]
    public void GenerateFloor_HasVertices()
    {
        var layout = MakeSmallLayout();
        var data = DungeonMeshBuilder.GenerateFloor(layout);
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Fact]
    public void GenerateWalls_HasVertices()
    {
        var layout = MakeSmallLayout();
        var data = DungeonMeshBuilder.GenerateWalls(layout);
        Assert.True(data.Vertices.Length > 0, "Should generate wall geometry");
    }

    [Fact]
    public void GenerateCeiling_HasVertices()
    {
        var layout = MakeSmallLayout();
        var data = DungeonMeshBuilder.GenerateCeiling(layout);
        Assert.True(data.Vertices.Length > 0);
    }

    [Fact]
    public void GenerateFloor_VertexStrideDivisible()
    {
        var layout = MakeSmallLayout();
        var data = DungeonMeshBuilder.GenerateFloor(layout);
        Assert.Equal(0, data.Vertices.Length % 9); // pos(3) + normal(3) + color(3)
    }

    [Fact]
    public void GenerateWalls_IndicesWithinBounds()
    {
        var layout = MakeSmallLayout();
        var data = DungeonMeshBuilder.GenerateWalls(layout);
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds");
    }

    [Fact]
    public void TileSize_IsReasonable()
    {
        Assert.True(DungeonMeshBuilder.TileSize > 0);
        Assert.True(DungeonMeshBuilder.WallHeight > 0);
    }
}
