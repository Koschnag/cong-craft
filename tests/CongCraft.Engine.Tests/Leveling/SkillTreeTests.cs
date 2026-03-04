using CongCraft.Engine.Leveling;

namespace CongCraft.Engine.Tests.Leveling;

public class SkillTreeTests
{
    [Fact]
    public void DefaultValues()
    {
        var skills = new SkillTree();
        Assert.Equal(0, skills.Strength);
        Assert.Equal(0, skills.Endurance);
        Assert.Equal(0, skills.Agility);
    }

    [Fact]
    public void TryAllocate_SucceedsWithSkillPoints()
    {
        var skills = new SkillTree();
        var level = new LevelComponent { SkillPoints = 3 };

        Assert.True(skills.TryAllocate(SkillType.Strength, level));
        Assert.Equal(1, skills.Strength);
        Assert.Equal(2, level.SkillPoints);
    }

    [Fact]
    public void TryAllocate_FailsWithNoPoints()
    {
        var skills = new SkillTree();
        var level = new LevelComponent { SkillPoints = 0 };

        Assert.False(skills.TryAllocate(SkillType.Strength, level));
        Assert.Equal(0, skills.Strength);
    }

    [Fact]
    public void TryAllocate_FailsAtMaxLevel()
    {
        var skills = new SkillTree { Strength = SkillTree.MaxSkillLevel };
        var level = new LevelComponent { SkillPoints = 5 };

        Assert.False(skills.TryAllocate(SkillType.Strength, level));
        Assert.Equal(5, level.SkillPoints); // Not consumed
    }

    [Fact]
    public void TotalAttackBonus_CalculatesCorrectly()
    {
        var skills = new SkillTree { Strength = 5 };
        Assert.Equal(15f, skills.TotalAttackBonus); // 5 * 3
    }

    [Fact]
    public void TotalHealthBonus_CalculatesCorrectly()
    {
        var skills = new SkillTree { Endurance = 3 };
        Assert.Equal(45f, skills.TotalHealthBonus); // 3 * 15
    }

    [Fact]
    public void TotalSpeedBonus_CalculatesCorrectly()
    {
        var skills = new SkillTree { Agility = 4 };
        Assert.Equal(2f, skills.TotalSpeedBonus); // 4 * 0.5
    }

    [Fact]
    public void TotalCooldownReduction_CalculatesCorrectly()
    {
        var skills = new SkillTree { Agility = 6 };
        Assert.Equal(0.3f, skills.TotalCooldownReduction, 0.001f); // 6 * 0.05
    }

    [Fact]
    public void AllocateAllSkillTypes()
    {
        var skills = new SkillTree();
        var level = new LevelComponent { SkillPoints = 6 };

        Assert.True(skills.TryAllocate(SkillType.Strength, level));
        Assert.True(skills.TryAllocate(SkillType.Strength, level));
        Assert.True(skills.TryAllocate(SkillType.Endurance, level));
        Assert.True(skills.TryAllocate(SkillType.Endurance, level));
        Assert.True(skills.TryAllocate(SkillType.Agility, level));
        Assert.True(skills.TryAllocate(SkillType.Agility, level));

        Assert.Equal(2, skills.Strength);
        Assert.Equal(2, skills.Endurance);
        Assert.Equal(2, skills.Agility);
        Assert.Equal(0, level.SkillPoints);
    }
}
