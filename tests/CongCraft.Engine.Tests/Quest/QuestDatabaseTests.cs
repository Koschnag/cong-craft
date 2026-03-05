using CongCraft.Engine.Quest;

namespace CongCraft.Engine.Tests.Quest;

public class QuestDatabaseTests
{
    [Fact]
    public void Get_BeastHunt()
    {
        var quest = QuestDatabase.Get("beast_hunt");
        Assert.NotNull(quest);
        Assert.Equal("Beast Hunt", quest.Title);
        Assert.Single(quest.Objectives);
        Assert.Equal(QuestObjectiveType.Kill, quest.Objectives[0].Type);
    }

    [Fact]
    public void Get_PeltCollector()
    {
        var quest = QuestDatabase.Get("pelt_collector");
        Assert.NotNull(quest);
        Assert.Equal("wolf_pelt", quest.Objectives[0].TargetId);
    }

    [Fact]
    public void Get_VillageTour()
    {
        var quest = QuestDatabase.Get("village_tour");
        Assert.NotNull(quest);
        Assert.Equal(2, quest.Objectives.Count);
        Assert.All(quest.Objectives, o => Assert.Equal(QuestObjectiveType.TalkTo, o.Type));
    }

    [Fact]
    public void Get_ReturnsNullForUnknown()
    {
        Assert.Null(QuestDatabase.Get("nonexistent"));
    }

    [Fact]
    public void All_HasExpectedQuestCount()
    {
        Assert.Equal(8, QuestDatabase.All.Count);
    }

    [Fact]
    public void AllQuests_HaveRewards()
    {
        foreach (var (_, quest) in QuestDatabase.All)
        {
            Assert.NotEmpty(quest.Rewards);
        }
    }

    [Fact]
    public void AllQuests_HaveXpRewards()
    {
        foreach (var (_, quest) in QuestDatabase.All)
        {
            Assert.True(quest.XpReward > 0, $"Quest '{quest.Id}' should give XP");
        }
    }

    [Fact]
    public void Get_DungeonDelve()
    {
        var quest = QuestDatabase.Get("dungeon_delve");
        Assert.NotNull(quest);
        Assert.Equal(150, quest.XpReward);
    }

    [Fact]
    public void Get_TrollSlayer()
    {
        var quest = QuestDatabase.Get("troll_slayer");
        Assert.NotNull(quest);
        Assert.Equal(200, quest.XpReward);
    }

    [Fact]
    public void Get_GatherMaterials_HasTwoObjectives()
    {
        var quest = QuestDatabase.Get("gather_materials");
        Assert.NotNull(quest);
        Assert.Equal(2, quest.Objectives.Count);
    }

    [Fact]
    public void Get_BoneCollector()
    {
        var quest = QuestDatabase.Get("bone_collector");
        Assert.NotNull(quest);
        Assert.Equal("bone", quest.Objectives[0].TargetId);
    }
}
