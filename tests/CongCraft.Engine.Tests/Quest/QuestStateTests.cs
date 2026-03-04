using CongCraft.Engine.Quest;

namespace CongCraft.Engine.Tests.Quest;

public class QuestStateTests
{
    private static QuestState MakeKillQuest(int required = 3)
    {
        return new QuestState
        {
            Quest = new QuestData
            {
                Id = "test_kill", Title = "Test Kill", Description = "Kill stuff",
                Objectives = new() { new QuestObjective { Id = "kill", Description = "Kill", Type = QuestObjectiveType.Kill, RequiredCount = required } }
            },
            Objectives = new()
            {
                new QuestObjective { Id = "kill", Description = "Kill", Type = QuestObjectiveType.Kill, RequiredCount = required }
            }
        };
    }

    [Fact]
    public void NewQuest_IsActive()
    {
        var quest = MakeKillQuest();
        Assert.Equal(QuestStatus.Active, quest.Status);
        Assert.False(quest.AllObjectivesComplete);
    }

    [Fact]
    public void ReportProgress_IncrementsCount()
    {
        var quest = MakeKillQuest(3);
        quest.ReportProgress(QuestObjectiveType.Kill);
        Assert.Equal(1, quest.Objectives[0].CurrentCount);
    }

    [Fact]
    public void ReportProgress_CompletesOnReach()
    {
        var quest = MakeKillQuest(2);
        quest.ReportProgress(QuestObjectiveType.Kill);
        quest.ReportProgress(QuestObjectiveType.Kill);
        Assert.True(quest.AllObjectivesComplete);
        Assert.Equal(QuestStatus.Completed, quest.Status);
    }

    [Fact]
    public void ReportProgress_ClampsToMax()
    {
        var quest = MakeKillQuest(1);
        quest.ReportProgress(QuestObjectiveType.Kill, amount: 5);
        Assert.Equal(1, quest.Objectives[0].CurrentCount);
    }

    [Fact]
    public void ReportProgress_WrongType_NoEffect()
    {
        var quest = MakeKillQuest();
        quest.ReportProgress(QuestObjectiveType.Collect);
        Assert.Equal(0, quest.Objectives[0].CurrentCount);
    }

    [Fact]
    public void ReportProgress_TargetFiltering()
    {
        var quest = new QuestState
        {
            Quest = new QuestData
            {
                Id = "test", Title = "Test", Description = "Test",
                Objectives = new() { new QuestObjective { Id = "collect_pelts", Description = "Pelts", Type = QuestObjectiveType.Collect, TargetId = "wolf_pelt", RequiredCount = 3 } }
            },
            Objectives = new()
            {
                new QuestObjective { Id = "collect_pelts", Description = "Pelts", Type = QuestObjectiveType.Collect, TargetId = "wolf_pelt", RequiredCount = 3 }
            }
        };

        quest.ReportProgress(QuestObjectiveType.Collect, "gold_coin"); // Wrong target
        Assert.Equal(0, quest.Objectives[0].CurrentCount);

        quest.ReportProgress(QuestObjectiveType.Collect, "wolf_pelt");
        Assert.Equal(1, quest.Objectives[0].CurrentCount);
    }

    [Fact]
    public void ReportProgress_InactiveQuest_NoEffect()
    {
        var quest = MakeKillQuest(1);
        quest.ReportProgress(QuestObjectiveType.Kill); // Completes
        Assert.Equal(QuestStatus.Completed, quest.Status);

        quest.Status = QuestStatus.TurnedIn;
        quest.Objectives[0].CurrentCount = 0; // Reset for test
        quest.ReportProgress(QuestObjectiveType.Kill); // Should not progress
        Assert.Equal(0, quest.Objectives[0].CurrentCount);
    }
}
