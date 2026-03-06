using System.Numerics;
using CongCraft.Engine.Level;

namespace CongCraft.Engine.Tests.Level;

public class LevelDataTests
{
    [Fact]
    public void LevelOneData_Create_ReturnsNonNull()
    {
        var level = LevelOneData.Create();
        Assert.NotNull(level);
    }

    [Fact]
    public void LevelOneData_Create_HasCorrectName()
    {
        var level = LevelOneData.Create();
        Assert.Contains("Ashvale", level.Name);
    }

    [Fact]
    public void LevelOneData_Create_HasHeightFeatures()
    {
        var level = LevelOneData.Create();
        Assert.True(level.HeightFeatures.Count > 0, "Level should have height features");
    }

    [Fact]
    public void LevelOneData_Create_HasPaths()
    {
        var level = LevelOneData.Create();
        Assert.True(level.Paths.Count > 0, "Level should have paths");
    }

    [Fact]
    public void LevelOneData_Create_HasNpcs()
    {
        var level = LevelOneData.Create();
        Assert.True(level.Npcs.Count > 0, "Level should have NPCs");
    }

    [Fact]
    public void LevelOneData_Create_HasEnemyZones()
    {
        var level = LevelOneData.Create();
        Assert.True(level.EnemyZones.Count > 0, "Level should have enemy zones");
    }

    [Fact]
    public void LevelOneData_Create_HasTrees()
    {
        var level = LevelOneData.Create();
        Assert.True(level.Trees.Count > 0, "Level should have trees");
    }

    [Fact]
    public void LevelOneData_Create_HasPlayerSpawn()
    {
        var level = LevelOneData.Create();
        Assert.NotEqual(Vector3.Zero, level.PlayerSpawn);
    }

    [Fact]
    public void LevelOneData_Create_HasTerrainZones()
    {
        var level = LevelOneData.Create();
        Assert.True(level.Zones.Count > 0, "Level should have terrain zones");
    }

    [Fact]
    public void LevelOneData_Create_HasTorches()
    {
        var level = LevelOneData.Create();
        Assert.True(level.Torches.Count > 0, "Level should have torches");
    }
}
