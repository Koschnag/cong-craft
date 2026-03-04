using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.ECS;

public class CombatComponentTests
{
    [Fact]
    public void Defaults_Reasonable()
    {
        var combat = new CombatComponent();
        Assert.Equal(10f, combat.AttackDamage);
        Assert.Equal(2.5f, combat.AttackRange);
        Assert.Equal(0.8f, combat.AttackCooldown);
        Assert.Equal(0.7f, combat.BlockDamageReduction);
    }

    [Fact]
    public void InitialState_NotInCombat()
    {
        var combat = new CombatComponent();
        Assert.False(combat.IsAttacking);
        Assert.False(combat.IsBlocking);
        Assert.False(combat.IsDodging);
    }

    [Fact]
    public void DodgeProperties_ConfigurableDefaults()
    {
        var combat = new CombatComponent();
        Assert.Equal(3f, combat.DodgeDistance);
        Assert.Equal(1.0f, combat.DodgeCooldown);
        Assert.Equal(0.3f, combat.DodgeDuration);
    }
}
