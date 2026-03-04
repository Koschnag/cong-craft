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
    public void All_HasThreeQuests()
    {
        Assert.Equal(3, QuestDatabase.All.Count);
    }

    [Fact]
    public void AllQuests_HaveRewards()
    {
        foreach (var (_, quest) in QuestDatabase.All)
        {
            Assert.NotEmpty(quest.Rewards);
        }
    }
}
