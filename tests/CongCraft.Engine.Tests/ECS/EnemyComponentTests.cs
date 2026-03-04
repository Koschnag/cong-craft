using System.Numerics;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.ECS;

public class EnemyComponentTests
{
    [Fact]
    public void DefaultState_IsPatrol()
    {
        var enemy = new EnemyComponent();
        Assert.Equal(EnemyState.Patrol, enemy.State);
    }

    [Fact]
    public void Defaults_Reasonable()
    {
        var enemy = new EnemyComponent();
        Assert.Equal(15f, enemy.DetectionRange);
        Assert.Equal(2.5f, enemy.AttackRange);
        Assert.Equal(2.5f, enemy.MoveSpeed);
        Assert.Equal(4f, enemy.ChaseSpeed);
        Assert.Equal(10f, enemy.PatrolRadius);
    }

    [Fact]
    public void InitialState_NotDead()
    {
        var enemy = new EnemyComponent();
        Assert.False(enemy.IsDead);
        Assert.Equal(0f, enemy.DeathTimer);
    }

    [Fact]
    public void AllStates_Exist()
    {
        Assert.Equal(4, Enum.GetValues<EnemyState>().Length);
        Assert.Contains(EnemyState.Patrol, Enum.GetValues<EnemyState>());
        Assert.Contains(EnemyState.Chase, Enum.GetValues<EnemyState>());
        Assert.Contains(EnemyState.Attack, Enum.GetValues<EnemyState>());
        Assert.Contains(EnemyState.Dead, Enum.GetValues<EnemyState>());
    }
}
