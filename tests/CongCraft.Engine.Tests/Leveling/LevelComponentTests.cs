using CongCraft.Engine.Leveling;

namespace CongCraft.Engine.Tests.Leveling;

public class LevelComponentTests
{
    [Fact]
    public void DefaultValues()
    {
        var level = new LevelComponent();
        Assert.Equal(1, level.Level);
        Assert.Equal(0, level.Experience);
        Assert.Equal(0, level.SkillPoints);
        Assert.False(level.IsSkillMenuOpen);
    }

    [Fact]
    public void XpToNextLevel_ScalesQuadratically()
    {
        var level = new LevelComponent();
        Assert.Equal(100, level.XpToNextLevel); // 1*1*50 + 50

        level.Level = 2;
        Assert.Equal(250, level.XpToNextLevel); // 2*2*50 + 50

        level.Level = 5;
        Assert.Equal(1300, level.XpToNextLevel); // 5*5*50 + 50
    }

    [Fact]
    public void AddExperience_GainsLevel()
    {
        var level = new LevelComponent();
        int gained = level.AddExperience(100); // Exactly enough for level 2

        Assert.Equal(1, gained);
        Assert.Equal(2, level.Level);
        Assert.Equal(0, level.Experience);
        Assert.Equal(2, level.SkillPoints);
    }

    [Fact]
    public void AddExperience_PartialXp()
    {
        var level = new LevelComponent();
        int gained = level.AddExperience(50);

        Assert.Equal(0, gained);
        Assert.Equal(1, level.Level);
        Assert.Equal(50, level.Experience);
    }

    [Fact]
    public void AddExperience_MultiplelevelsAtOnce()
    {
        var level = new LevelComponent();
        // Level 1->2: 100 XP, Level 2->3: 250 XP, total: 350
        int gained = level.AddExperience(350);

        Assert.Equal(2, gained);
        Assert.Equal(3, level.Level);
        Assert.Equal(0, level.Experience);
        Assert.Equal(4, level.SkillPoints); // 2 per level * 2 levels
    }

    [Fact]
    public void AddExperience_OverflowCarries()
    {
        var level = new LevelComponent();
        int gained = level.AddExperience(120); // 100 needed, 20 overflow

        Assert.Equal(1, gained);
        Assert.Equal(2, level.Level);
        Assert.Equal(20, level.Experience);
    }

    [Fact]
    public void LevelProgress_CalculatesCorrectly()
    {
        var level = new LevelComponent();
        Assert.Equal(0f, level.LevelProgress);

        level.AddExperience(50); // 50/100
        Assert.Equal(0.5f, level.LevelProgress);
    }
}
