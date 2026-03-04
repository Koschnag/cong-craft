using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.ECS;

public class WorldTests
{
    [Fact]
    public void CreateEntity_ReturnsUniqueIds()
    {
        var world = new World();
        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        Assert.NotEqual(e1.Id, e2.Id);
    }

    [Fact]
    public void CreateEntity_IncrementsEntityCount()
    {
        var world = new World();
        Assert.Equal(0, world.EntityCount);
        world.CreateEntity();
        Assert.Equal(1, world.EntityCount);
        world.CreateEntity();
        Assert.Equal(2, world.EntityCount);
    }

    [Fact]
    public void DestroyEntity_DecrementsCount()
    {
        var world = new World();
        var e = world.CreateEntity();
        world.DestroyEntity(e);
        Assert.Equal(0, world.EntityCount);
    }

    [Fact]
    public void AddComponent_And_GetComponent()
    {
        var world = new World();
        var e = world.CreateEntity();
        var comp = new TestComponent { Value = 42 };
        world.AddComponent(e, comp);

        var retrieved = world.GetComponent<TestComponent>(e);
        Assert.Equal(42, retrieved.Value);
    }

    [Fact]
    public void GetComponent_Missing_Throws()
    {
        var world = new World();
        var e = world.CreateEntity();
        Assert.Throws<InvalidOperationException>(() => world.GetComponent<TestComponent>(e));
    }

    [Fact]
    public void HasComponent_ReturnsCorrectly()
    {
        var world = new World();
        var e = world.CreateEntity();
        Assert.False(world.HasComponent<TestComponent>(e));
        world.AddComponent(e, new TestComponent());
        Assert.True(world.HasComponent<TestComponent>(e));
    }

    [Fact]
    public void Query_ReturnsOnlyMatchingEntities()
    {
        var world = new World();
        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        world.AddComponent(e1, new TestComponent { Value = 1 });
        world.AddComponent(e3, new TestComponent { Value = 3 });

        var results = world.Query<TestComponent>().ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Component.Value == 1);
        Assert.Contains(results, r => r.Component.Value == 3);
    }

    [Fact]
    public void Query_TwoComponents_ReturnsIntersection()
    {
        var world = new World();
        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();

        world.AddComponent(e1, new TestComponent { Value = 1 });
        world.AddComponent(e1, new OtherComponent { Name = "A" });
        world.AddComponent(e2, new TestComponent { Value = 2 });
        // e2 has no OtherComponent

        var results = world.Query<TestComponent, OtherComponent>().ToList();
        Assert.Single(results);
        Assert.Equal(1, results[0].C1.Value);
        Assert.Equal("A", results[0].C2.Name);
    }

    [Fact]
    public void Singleton_SetAndGet()
    {
        var world = new World();
        var comp = new TestComponent { Value = 99 };
        world.SetSingleton(comp);

        var retrieved = world.GetSingleton<TestComponent>();
        Assert.Equal(99, retrieved.Value);
    }

    [Fact]
    public void Singleton_Missing_Throws()
    {
        var world = new World();
        Assert.Throws<InvalidOperationException>(() => world.GetSingleton<TestComponent>());
    }

    private class TestComponent : IComponent { public int Value { get; set; } }
    private class OtherComponent : IComponent { public string Name { get; set; } = ""; }
}
