using CongCraft.Engine.Boss;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.Boss;

public class BossIntegrationTests
{
    [Fact]
    public void BossComponent_CanTrackAttackProgress()
    {
        var boss = new BossComponent { BossId = "forest_guardian", Name = "Forest Guardian" };
        boss.CurrentAttack = BossAttackType.Slam;
        boss.AttackProgress = 0.5f;

        Assert.Equal(BossAttackType.Slam, boss.CurrentAttack);
        Assert.Equal(0.5f, boss.AttackProgress);
    }

    [Fact]
    public void BossComponent_PhaseTransition()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test", MaxPhases = 3 };
        Assert.Equal(0, boss.CurrentPhase);

        boss.CurrentPhase = 1;
        boss.State = BossState.Enraged;
        Assert.Equal(1, boss.CurrentPhase);
        Assert.Equal(BossState.Enraged, boss.State);

        boss.CurrentPhase = 2;
        Assert.Equal(2, boss.CurrentPhase);
    }

    [Fact]
    public void BossComponent_TimerManagement()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        boss.AttackTimer = 1.5f;
        boss.SpecialAttackTimer = 5f;

        Assert.Equal(1.5f, boss.AttackTimer);
        Assert.Equal(5f, boss.SpecialAttackTimer);

        // Simulate timer tick
        boss.AttackTimer = MathF.Max(0, boss.AttackTimer - 0.5f);
        Assert.Equal(1f, boss.AttackTimer);
    }

    [Fact]
    public void BossComponent_ArenaRadius_Default()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(20f, boss.ArenaRadius);
    }

    [Fact]
    public void HealthComponent_WorksWithBossHealth()
    {
        var health = new HealthComponent { Current = 500f, Max = 500f };
        Assert.True(health.IsAlive);
        Assert.Equal(1f, health.Percentage);

        health.TakeDamage(170f);
        Assert.Equal(330f, health.Current);
        Assert.True(health.Percentage <= 0.66f); // Should trigger phase 1

        health.TakeDamage(165f);
        Assert.Equal(165f, health.Current);
        Assert.True(health.Percentage <= 0.33f); // Should trigger phase 2
    }

    [Fact]
    public void BossData_SpawnPositions_AreUnique()
    {
        var positions = BossDatabase.All.Values.Select(b => b.SpawnPosition).ToList();
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                Assert.NotEqual(positions[i], positions[j]);
            }
        }
    }
}
