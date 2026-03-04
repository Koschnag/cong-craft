using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Leveling;

/// <summary>
/// Tracks experience points, current level, and available skill points.
/// </summary>
public sealed class LevelComponent : IComponent
{
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int SkillPoints { get; set; }

    /// <summary>
    /// XP required to reach the next level. Scales quadratically.
    /// </summary>
    public int XpToNextLevel => Level * Level * 50 + 50;

    /// <summary>
    /// Adds XP and returns the number of levels gained.
    /// </summary>
    public int AddExperience(int xp)
    {
        Experience += xp;
        int levelsGained = 0;

        while (Experience >= XpToNextLevel)
        {
            Experience -= XpToNextLevel;
            Level++;
            SkillPoints += 2;
            levelsGained++;
        }

        return levelsGained;
    }

    /// <summary>
    /// Progress to next level as 0..1 fraction.
    /// </summary>
    public float LevelProgress => XpToNextLevel > 0 ? (float)Experience / XpToNextLevel : 0f;

    public bool IsSkillMenuOpen { get; set; }
}
