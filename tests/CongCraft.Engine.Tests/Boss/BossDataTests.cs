using CongCraft.Engine.Boss;

namespace CongCraft.Engine.Tests.Boss;

public class BossDataTests
{
    [Fact]
    public void BossDatabase_HasTwoBosses()
    {
        Assert.Equal(2, BossDatabase.All.Count);
    }

    [Fact]
    public void ForestGuardian_HasCorrectStats()
    {
        var boss = BossDatabase.Get("forest_guardian");
        Assert.NotNull(boss);
        Assert.Equal("Forest Guardian", boss.Name);
        Assert.Equal(500f, boss.Health);
        Assert.Equal(3, boss.MaxPhases);
        Assert.Equal(2, boss.PhaseThresholds.Length);
        Assert.Equal(0.66f, boss.PhaseThresholds[0]);
        Assert.Equal(0.33f, boss.PhaseThresholds[1]);
        Assert.Equal(2.5f, boss.Scale);
    }

    [Fact]
    public void ShadowKnight_HasCorrectStats()
    {
        var boss = BossDatabase.Get("shadow_knight");
        Assert.NotNull(boss);
        Assert.Equal("Shadow Knight", boss.Name);
        Assert.Equal(400f, boss.Health);
        Assert.Equal(2, boss.MaxPhases);
        Assert.Single(boss.PhaseThresholds);
        Assert.Equal(0.5f, boss.PhaseThresholds[0]);
        Assert.Equal(2f, boss.Scale);
    }

    [Fact]
    public void Get_ReturnsNull_ForUnknownId()
    {
        Assert.Null(BossDatabase.Get("nonexistent_boss"));
    }

    [Fact]
    public void AllBosses_HavePositiveHealth()
    {
        foreach (var boss in BossDatabase.All.Values)
        {
            Assert.True(boss.Health > 0, $"{boss.Name} should have positive health");
        }
    }

    [Fact]
    public void AllBosses_HavePositiveMeleeRange()
    {
        foreach (var boss in BossDatabase.All.Values)
        {
            Assert.True(boss.MeleeRange > 0, $"{boss.Name} should have positive melee range");
        }
    }

    [Fact]
    public void AllBosses_HavePositiveScale()
    {
        foreach (var boss in BossDatabase.All.Values)
        {
            Assert.True(boss.Scale > 0, $"{boss.Name} should have positive scale");
        }
    }

    [Fact]
    public void AllBosses_HaveValidPhaseThresholds()
    {
        foreach (var boss in BossDatabase.All.Values)
        {
            foreach (var threshold in boss.PhaseThresholds)
            {
                Assert.True(threshold > 0 && threshold < 1,
                    $"{boss.Name} phase threshold {threshold} should be between 0 and 1");
            }
        }
    }

    [Fact]
    public void ForestGuardian_HasCombatValues()
    {
        var boss = BossDatabase.Get("forest_guardian")!;
        Assert.Equal(25f, boss.MeleeDamage);
        Assert.Equal(35f, boss.SlamDamage);
        Assert.Equal(6f, boss.SlamRadius);
        Assert.Equal(30f, boss.ChargeDamage);
        Assert.Equal(12f, boss.ChargeSpeed);
    }

    [Fact]
    public void ShadowKnight_HasCombatValues()
    {
        var boss = BossDatabase.Get("shadow_knight")!;
        Assert.Equal(30f, boss.MeleeDamage);
        Assert.Equal(20f, boss.SlamDamage);
        Assert.Equal(40f, boss.ChargeDamage);
        Assert.Equal(15f, boss.ChargeSpeed);
    }
}
