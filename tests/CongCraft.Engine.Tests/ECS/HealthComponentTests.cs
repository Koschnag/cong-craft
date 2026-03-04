using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.ECS;

public class HealthComponentTests
{
    [Fact]
    public void NewComponent_FullHealth()
    {
        var health = new HealthComponent { Current = 100, Max = 100 };
        Assert.True(health.IsAlive);
        Assert.Equal(1f, health.Percentage);
    }

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var health = new HealthComponent { Current = 100, Max = 100 };
        health.TakeDamage(30);
        Assert.Equal(70, health.Current);
        Assert.True(health.IsAlive);
    }

    [Fact]
    public void TakeDamage_ClampsToZero()
    {
        var health = new HealthComponent { Current = 20, Max = 100 };
        health.TakeDamage(50);
        Assert.Equal(0, health.Current);
        Assert.False(health.IsAlive);
    }

    [Fact]
    public void TakeDamage_SetsDamageFlash()
    {
        var health = new HealthComponent { Current = 100, Max = 100 };
        health.TakeDamage(10);
        Assert.Equal(10f, health.LastDamageAmount);
        Assert.Equal(0.3f, health.DamageFlashTimer);
    }

    [Fact]
    public void Heal_IncreasesHealth()
    {
        var health = new HealthComponent { Current = 50, Max = 100 };
        health.Heal(30);
        Assert.Equal(80, health.Current);
    }

    [Fact]
    public void Heal_ClampsToMax()
    {
        var health = new HealthComponent { Current = 90, Max = 100 };
        health.Heal(50);
        Assert.Equal(100, health.Current);
    }

    [Fact]
    public void Percentage_CalculatesCorrectly()
    {
        var health = new HealthComponent { Current = 25, Max = 50 };
        Assert.Equal(0.5f, health.Percentage);
    }

    [Fact]
    public void Percentage_ZeroMax_ReturnsZero()
    {
        var health = new HealthComponent { Current = 0, Max = 0 };
        Assert.Equal(0f, health.Percentage);
    }
}
