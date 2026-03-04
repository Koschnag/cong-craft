namespace CongCraft.Engine.Quest;

/// <summary>
/// Defines a quest with objectives and rewards.
/// </summary>
public sealed class QuestData
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public List<QuestObjective> Objectives { get; init; } = new();
    public List<QuestReward> Rewards { get; init; } = new();
}

/// <summary>
/// A single objective within a quest (kill, collect, talk).
/// </summary>
public sealed class QuestObjective
{
    public required string Id { get; init; }
    public required string Description { get; init; }
    public QuestObjectiveType Type { get; init; }
    public string? TargetId { get; init; }
    public int RequiredCount { get; init; } = 1;
    public int CurrentCount { get; set; }
    public bool IsComplete => CurrentCount >= RequiredCount;
}

/// <summary>
/// Reward given upon quest completion.
/// </summary>
public sealed class QuestReward
{
    public required string ItemId { get; init; }
    public int Quantity { get; init; } = 1;
}

public enum QuestObjectiveType
{
    Kill,
    Collect,
    TalkTo,
    Explore
}

public enum QuestStatus
{
    Available,
    Active,
    Completed,
    TurnedIn
}
