using CongCraft.Engine.Quest;

namespace CongCraft.Engine.Tests.Quest;

public class QuestJournalTests
{
    private static QuestData MakeQuest(string id = "test") => new()
    {
        Id = id, Title = $"Quest {id}", Description = "Test quest",
        Objectives = new()
        {
            new QuestObjective { Id = "obj", Description = "Do thing", Type = QuestObjectiveType.Kill, RequiredCount = 1 }
        },
        Rewards = new()
        {
            new QuestReward { ItemId = "gold_coin", Quantity = 5 }
        }
    };

    [Fact]
    public void NewJournal_IsEmpty()
    {
        var journal = new QuestJournal();
        Assert.Empty(journal.ActiveQuests);
        Assert.Empty(journal.CompletedQuests);
    }

    [Fact]
    public void AcceptQuest_AddsToActive()
    {
        var journal = new QuestJournal();
        Assert.True(journal.AcceptQuest(MakeQuest()));
        Assert.Single(journal.ActiveQuests);
    }

    [Fact]
    public void AcceptQuest_DuplicateRejected()
    {
        var journal = new QuestJournal();
        journal.AcceptQuest(MakeQuest("q1"));
        Assert.False(journal.AcceptQuest(MakeQuest("q1")));
        Assert.Single(journal.ActiveQuests);
    }

    [Fact]
    public void HasQuest_TrueForActive()
    {
        var journal = new QuestJournal();
        journal.AcceptQuest(MakeQuest("q1"));
        Assert.True(journal.HasQuest("q1"));
        Assert.False(journal.HasQuest("q2"));
    }

    [Fact]
    public void GetActiveQuest_ReturnsQuest()
    {
        var journal = new QuestJournal();
        journal.AcceptQuest(MakeQuest("q1"));
        Assert.NotNull(journal.GetActiveQuest("q1"));
        Assert.Null(journal.GetActiveQuest("q2"));
    }

    [Fact]
    public void TurnInQuest_MovesToCompleted()
    {
        var journal = new QuestJournal();
        var quest = MakeQuest("q1");
        journal.AcceptQuest(quest);

        // Must be completed first
        Assert.False(journal.TurnInQuest("q1")); // Still active, not completed

        journal.ActiveQuests[0].ReportProgress(QuestObjectiveType.Kill);
        Assert.True(journal.TurnInQuest("q1"));

        Assert.Empty(journal.ActiveQuests);
        Assert.Single(journal.CompletedQuests);
        Assert.True(journal.HasQuest("q1")); // Still tracked
    }

    [Fact]
    public void ReportProgress_DelegatestoActiveQuests()
    {
        var journal = new QuestJournal();
        journal.AcceptQuest(MakeQuest("q1"));
        journal.ReportProgress(QuestObjectiveType.Kill);
        Assert.Equal(1, journal.ActiveQuests[0].Objectives[0].CurrentCount);
    }

    [Fact]
    public void AcceptQuest_CopiesObjectives()
    {
        var journal = new QuestJournal();
        var quest = MakeQuest();
        journal.AcceptQuest(quest);

        // Modifying journal objectives shouldn't affect quest data
        journal.ActiveQuests[0].Objectives[0].CurrentCount = 1;
        Assert.Equal(0, quest.Objectives[0].CurrentCount);
    }

    [Fact]
    public void MultipleQuests_IndependentTracking()
    {
        var journal = new QuestJournal();
        journal.AcceptQuest(MakeQuest("q1"));
        journal.AcceptQuest(MakeQuest("q2"));
        Assert.Equal(2, journal.ActiveQuests.Count);

        journal.ReportProgress(QuestObjectiveType.Kill);
        Assert.True(journal.ActiveQuests[0].AllObjectivesComplete);
        Assert.True(journal.ActiveQuests[1].AllObjectivesComplete);
    }
}
