using CongCraft.Engine.Dungeon;

namespace CongCraft.Engine.Tests.Dungeon;

public class DungeonGeneratorTests
{
    [Fact]
    public void Generate_CreatesRooms()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(5);
        Assert.True(layout.Rooms.Count >= 2, "Should generate at least 2 rooms");
    }

    [Fact]
    public void Generate_HasFloorTiles()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(5);
        Assert.True(layout.FloorTileCount > 0, "Should have walkable floor tiles");
    }

    [Fact]
    public void Generate_HasEntranceAndExit()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(5);
        Assert.Equal(TileType.Entrance, layout.Grid[layout.Entrance.X, layout.Entrance.Y]);
        Assert.Equal(TileType.Exit, layout.Grid[layout.Exit.X, layout.Exit.Y]);
    }

    [Fact]
    public void Generate_EntranceAndExitDifferent()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(5);
        Assert.NotEqual(layout.Entrance, layout.Exit);
    }

    [Fact]
    public void Generate_Deterministic()
    {
        var gen1 = new DungeonGenerator(40, 40, seed: 123);
        var layout1 = gen1.Generate(4);
        var gen2 = new DungeonGenerator(40, 40, seed: 123);
        var layout2 = gen2.Generate(4);

        Assert.Equal(layout1.Rooms.Count, layout2.Rooms.Count);
        Assert.Equal(layout1.FloorTileCount, layout2.FloorTileCount);
    }

    [Fact]
    public void Generate_DifferentSeeds_DifferentLayouts()
    {
        var gen1 = new DungeonGenerator(40, 40, seed: 1);
        var layout1 = gen1.Generate(4);
        var gen2 = new DungeonGenerator(40, 40, seed: 999);
        var layout2 = gen2.Generate(4);

        // Different seeds should produce different room counts or positions
        bool different = layout1.Rooms.Count != layout2.Rooms.Count ||
                        layout1.FloorTileCount != layout2.FloorTileCount;
        Assert.True(different, "Different seeds should produce different layouts");
    }

    [Fact]
    public void IsWalkable_TrueForFloor()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(4);
        Assert.True(layout.IsWalkable(layout.Entrance.X, layout.Entrance.Y));
    }

    [Fact]
    public void IsWalkable_FalseOutOfBounds()
    {
        var gen = new DungeonGenerator(40, 40, seed: 42);
        var layout = gen.Generate(4);
        Assert.False(layout.IsWalkable(-1, 0));
        Assert.False(layout.IsWalkable(0, -1));
        Assert.False(layout.IsWalkable(40, 0));
    }

    [Fact]
    public void Room_Overlap_Detection()
    {
        var a = new DungeonRoom(0, 0, 5, 5);
        var b = new DungeonRoom(3, 3, 5, 5);
        Assert.True(a.Overlaps(b));

        var c = new DungeonRoom(10, 10, 3, 3);
        Assert.False(a.Overlaps(c));
    }

    [Fact]
    public void Room_Center_Calculation()
    {
        var room = new DungeonRoom(2, 4, 6, 8);
        Assert.Equal((5, 8), room.Center);
    }
}
