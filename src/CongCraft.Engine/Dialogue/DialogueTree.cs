namespace CongCraft.Engine.Dialogue;

/// <summary>
/// A complete dialogue conversation with named nodes.
/// </summary>
public sealed class DialogueTree
{
    public required string Id { get; init; }
    public required string StartNodeId { get; init; }
    private readonly Dictionary<string, DialogueNode> _nodes = new();

    public DialogueTree AddNode(DialogueNode node)
    {
        _nodes[node.Id] = node;
        return this;
    }

    public DialogueNode? GetNode(string id) => _nodes.GetValueOrDefault(id);
    public DialogueNode? StartNode => GetNode(StartNodeId);
    public int NodeCount => _nodes.Count;
}
