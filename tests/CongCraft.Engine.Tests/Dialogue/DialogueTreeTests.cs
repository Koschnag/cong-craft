using CongCraft.Engine.Dialogue;

namespace CongCraft.Engine.Tests.Dialogue;

public class DialogueTreeTests
{
    [Fact]
    public void AddNode_CanRetrieve()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "start" };
        tree.AddNode(new DialogueNode { Id = "start", SpeakerName = "NPC", Text = "Hello!" });
        Assert.NotNull(tree.GetNode("start"));
        Assert.Equal("Hello!", tree.GetNode("start")!.Text);
    }

    [Fact]
    public void StartNode_ReturnsFirstNode()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "greeting" };
        tree.AddNode(new DialogueNode { Id = "greeting", SpeakerName = "NPC", Text = "Hi!" });
        Assert.Equal("Hi!", tree.StartNode!.Text);
    }

    [Fact]
    public void GetNode_ReturnsNullForMissing()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "start" };
        Assert.Null(tree.GetNode("nonexistent"));
    }

    [Fact]
    public void NodeCount_TracksNodes()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "a" };
        tree.AddNode(new DialogueNode { Id = "a", SpeakerName = "NPC", Text = "A" });
        tree.AddNode(new DialogueNode { Id = "b", SpeakerName = "NPC", Text = "B" });
        Assert.Equal(2, tree.NodeCount);
    }

    [Fact]
    public void AddNode_Chainable()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "a" }
            .AddNode(new DialogueNode { Id = "a", SpeakerName = "NPC", Text = "A" })
            .AddNode(new DialogueNode { Id = "b", SpeakerName = "NPC", Text = "B" });
        Assert.Equal(2, tree.NodeCount);
    }
}
