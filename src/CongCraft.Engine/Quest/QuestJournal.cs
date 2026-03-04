using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Quest;

/// <summary>
/// Component that tracks all quests for an entity (typically the player).
/// </summary>
public sealed class QuestJournal : IComponent
{
    public List<QuestState> ActiveQuests { get; } = new();
    public List<QuestState> CompletedQuests { get; } = new();

    public bool HasQuest(string questId) =>
        ActiveQuests.Exists(q => q.Quest.Id == questId) ||
        CompletedQuests.Exists(q => q.Quest.Id == questId);

    public QuestState? GetActiveQuest(string questId) =>
        ActiveQuests.Find(q => q.Quest.Id == questId);

    public bool AcceptQuest(QuestData quest)
    {
        if (HasQuest(quest.Id)) return false;

        var objectives = quest.Objectives.Select(o => new QuestObjective
        {
            Id = o.Id,
            Description = o.Description,
            Type = o.Type,
            TargetId = o.TargetId,
            RequiredCount = o.RequiredCount,
            CurrentCount = 0
        }).ToList();

        ActiveQuests.Add(new QuestState
        {
            Quest = quest,
            Objectives = objectives
        });
        return true;
    }

    public bool TurnInQuest(string questId)
    {
        var quest = ActiveQuests.Find(q => q.Quest.Id == questId && q.Status == QuestStatus.Completed);
        if (quest == null) return false;

        quest.Status = QuestStatus.TurnedIn;
        ActiveQuests.Remove(quest);
        CompletedQuests.Add(quest);
        return true;
    }

    /// <summary>
    /// Reports progress across all active quests.
    /// </summary>
    public void ReportProgress(QuestObjectiveType type, string? targetId = null, int amount = 1)
    {
        foreach (var quest in ActiveQuests)
            quest.ReportProgress(type, targetId, amount);
    }
}
