namespace CongCraft.Engine.Quest;

/// <summary>
/// Tracks the runtime state of a single quest instance.
/// </summary>
public sealed class QuestState
{
    public required QuestData Quest { get; init; }
    public QuestStatus Status { get; set; } = QuestStatus.Active;
    public List<QuestObjective> Objectives { get; init; } = new();

    public bool AllObjectivesComplete => Objectives.TrueForAll(o => o.IsComplete);

    /// <summary>
    /// Reports progress on an objective (e.g., enemy killed, item collected).
    /// </summary>
    public bool ReportProgress(QuestObjectiveType type, string? targetId = null, int amount = 1)
    {
        if (Status != QuestStatus.Active) return false;

        bool progressed = false;
        foreach (var obj in Objectives)
        {
            if (obj.IsComplete) continue;
            if (obj.Type != type) continue;
            if (targetId != null && obj.TargetId != targetId) continue;

            obj.CurrentCount = Math.Min(obj.RequiredCount, obj.CurrentCount + amount);
            progressed = true;
        }

        if (AllObjectivesComplete)
            Status = QuestStatus.Completed;

        return progressed;
    }
}
