using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.ECS;

public class WorldExtendedTests
{
    [Fact]
    public void HasEntity_TrueForExisting()
    {
        var world = new World();
        var entity = world.CreateEntity();
        Assert.True(world.HasEntity(entity));
    }

    [Fact]
    public void HasEntity_FalseAfterDestroy()
    {
        var world = new World();
        var entity = world.CreateEntity();
        world.DestroyEntity(entity);
        Assert.False(world.HasEntity(entity));
    }

    [Fact]
    public void HasEntity_FalseForNonexistent()
    {
        var world = new World();
        Assert.False(world.HasEntity(new Entity(999)));
    }
}
