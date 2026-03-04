namespace CongCraft.Engine.Dialogue;

/// <summary>
/// A single node in a dialogue tree. Contains text and optional player choices.
/// </summary>
public sealed class DialogueNode
{
    public required string Id { get; init; }
    public required string SpeakerName { get; init; }
    public required string Text { get; init; }
    public List<DialogueChoice> Choices { get; init; } = new();

    /// <summary>
    /// If no choices, this is the next node ID (null = end dialogue).
    /// </summary>
    public string? NextNodeId { get; init; }
}

/// <summary>
/// A player choice that leads to another dialogue node.
/// </summary>
public sealed class DialogueChoice
{
    public required string Text { get; init; }
    public required string NextNodeId { get; init; }

    /// <summary>
    /// Optional condition: item required in inventory to show this choice.
    /// </summary>
    public string? RequiredItemId { get; init; }

    /// <summary>
    /// Optional: item given to player when this choice is selected.
    /// </summary>
    public string? RewardItemId { get; init; }
    public int RewardQuantity { get; init; } = 1;
}
