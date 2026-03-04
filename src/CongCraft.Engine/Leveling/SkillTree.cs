using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Leveling;

/// <summary>
/// Tracks skill point allocation across three attributes.
/// Each point in a skill provides specific stat bonuses.
/// </summary>
public sealed class SkillTree : IComponent
{
    public int Strength { get; set; }     // +3 attack damage per point
    public int Endurance { get; set; }    // +15 max health per point
    public int Agility { get; set; }      // +0.5 move speed, -0.05s attack cooldown per point

    public const int MaxSkillLevel = 20;
    public const float StrengthDamageBonus = 3f;
    public const float EnduranceHealthBonus = 15f;
    public const float AgilitySpeedBonus = 0.5f;
    public const float AgilityCooldownReduction = 0.05f;

    /// <summary>
    /// Tries to allocate a skill point. Returns true if successful.
    /// </summary>
    public bool TryAllocate(SkillType skill, LevelComponent level)
    {
        if (level.SkillPoints <= 0) return false;

        switch (skill)
        {
            case SkillType.Strength when Strength < MaxSkillLevel:
                Strength++;
                level.SkillPoints--;
                return true;
            case SkillType.Endurance when Endurance < MaxSkillLevel:
                Endurance++;
                level.SkillPoints--;
                return true;
            case SkillType.Agility when Agility < MaxSkillLevel:
                Agility++;
                level.SkillPoints--;
                return true;
            default:
                return false;
        }
    }

    public float TotalAttackBonus => Strength * StrengthDamageBonus;
    public float TotalHealthBonus => Endurance * EnduranceHealthBonus;
    public float TotalSpeedBonus => Agility * AgilitySpeedBonus;
    public float TotalCooldownReduction => Agility * AgilityCooldownReduction;
}

public enum SkillType
{
    Strength,
    Endurance,
    Agility
}
