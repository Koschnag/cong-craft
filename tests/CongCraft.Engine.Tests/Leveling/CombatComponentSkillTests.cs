using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.Leveling;

public class CombatComponentSkillTests
{
    [Fact]
    public void EffectiveAttackDamage_IncludesSkillBonus()
    {
        var combat = new CombatComponent { AttackDamage = 15f, SkillAttackBonus = 9f };
        Assert.Equal(24f, combat.EffectiveAttackDamage);
    }

    [Fact]
    public void EffectiveAttackCooldown_ReducedBySkill()
    {
        var combat = new CombatComponent { AttackCooldown = 0.8f, SkillCooldownReduction = 0.3f };
        Assert.Equal(0.5f, combat.EffectiveAttackCooldown, 0.001f);
    }

    [Fact]
    public void EffectiveAttackCooldown_MinimumCap()
    {
        var combat = new CombatComponent { AttackCooldown = 0.5f, SkillCooldownReduction = 1.0f };
        Assert.Equal(0.2f, combat.EffectiveAttackCooldown); // Capped at 0.2
    }

    [Fact]
    public void HealthComponent_EffectiveMax_IncludesSkillBonus()
    {
        var health = new HealthComponent { Max = 100f, SkillMaxHealthBonus = 45f };
        Assert.Equal(145f, health.EffectiveMax);
    }

    [Fact]
    public void PlayerComponent_EffectiveSpeed_IncludesSkillBonus()
    {
        var player = new PlayerComponent { MoveSpeed = 5f, RunSpeed = 10f, SkillSpeedBonus = 2f };
        Assert.Equal(7f, player.EffectiveMoveSpeed);
        Assert.Equal(12f, player.EffectiveRunSpeed);
    }
}
