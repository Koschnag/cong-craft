using CongCraft.Engine.Boss;

namespace CongCraft.Engine.Tests.Boss;

public class BossComponentTests
{
    [Fact]
    public void DefaultState_IsIdle()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(BossState.Idle, boss.State);
    }

    [Fact]
    public void DefaultPhase_IsZero()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(0, boss.CurrentPhase);
    }

    [Fact]
    public void IsActivated_DefaultsFalse()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.False(boss.IsActivated);
    }

    [Fact]
    public void ActivationRadius_DefaultsTo15()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(15f, boss.ActivationRadius);
    }

    [Fact]
    public void CurrentAttack_DefaultsToNone()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(BossAttackType.None, boss.CurrentAttack);
    }

    [Fact]
    public void PhaseThresholds_HasDefaults()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(2, boss.PhaseThresholds.Length);
        Assert.Equal(0.66f, boss.PhaseThresholds[0]);
        Assert.Equal(0.33f, boss.PhaseThresholds[1]);
    }

    [Fact]
    public void StateTransitions_Work()
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        Assert.Equal(BossState.Idle, boss.State);

        boss.IsActivated = true;
        boss.State = BossState.Activated;
        Assert.Equal(BossState.Activated, boss.State);

        boss.State = BossState.Attacking;
        boss.CurrentAttack = BossAttackType.Melee;
        Assert.Equal(BossState.Attacking, boss.State);
        Assert.Equal(BossAttackType.Melee, boss.CurrentAttack);

        boss.State = BossState.Dead;
        Assert.Equal(BossState.Dead, boss.State);
    }

    [Theory]
    [InlineData(BossState.Idle)]
    [InlineData(BossState.Activated)]
    [InlineData(BossState.Attacking)]
    [InlineData(BossState.SpecialAttack)]
    [InlineData(BossState.PhaseTransition)]
    [InlineData(BossState.Enraged)]
    [InlineData(BossState.Dead)]
    public void AllBossStates_Exist(BossState state)
    {
        var boss = new BossComponent { BossId = "test", Name = "Test" };
        boss.State = state;
        Assert.Equal(state, boss.State);
    }
}
