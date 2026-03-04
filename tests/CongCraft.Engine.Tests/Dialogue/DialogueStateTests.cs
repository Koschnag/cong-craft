using CongCraft.Engine.Dialogue;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.Dialogue;

public class DialogueStateTests
{
    private static DialogueTree MakeTree()
    {
        return new DialogueTree { Id = "test", StartNodeId = "start" }
            .AddNode(new DialogueNode
            {
                Id = "start", SpeakerName = "NPC", Text = "Hello!",
                Choices = new()
                {
                    new() { Text = "Option A", NextNodeId = "node_a" },
                    new() { Text = "Option B", NextNodeId = "node_b" }
                }
            })
            .AddNode(new DialogueNode { Id = "node_a", SpeakerName = "NPC", Text = "You chose A!", NextNodeId = null })
            .AddNode(new DialogueNode { Id = "node_b", SpeakerName = "NPC", Text = "You chose B!", NextNodeId = "start" });
    }

    [Fact]
    public void Start_ActivatesDialogue()
    {
        var state = new DialogueState();
        var tree = MakeTree();
        state.Start(new Entity(1), tree);

        Assert.True(state.IsActive);
        Assert.Equal("start", state.CurrentNode?.Id);
        Assert.Equal(0, state.SelectedChoice);
    }

    [Fact]
    public void End_DeactivatesDialogue()
    {
        var state = new DialogueState();
        state.Start(new Entity(1), MakeTree());
        state.End();

        Assert.False(state.IsActive);
        Assert.Null(state.CurrentNode);
        Assert.Null(state.CurrentTree);
    }

    [Fact]
    public void GoToNode_NavigatesToExistingNode()
    {
        var state = new DialogueState();
        state.Start(new Entity(1), MakeTree());
        state.GoToNode("node_a");

        Assert.Equal("node_a", state.CurrentNode?.Id);
        Assert.Equal("You chose A!", state.CurrentNode?.Text);
    }

    [Fact]
    public void GoToNode_EndsOnMissingNode()
    {
        var state = new DialogueState();
        state.Start(new Entity(1), MakeTree());
        state.GoToNode("nonexistent");

        Assert.False(state.IsActive);
    }

    [Fact]
    public void GoToNode_ResetsSelectedChoice()
    {
        var state = new DialogueState();
        state.Start(new Entity(1), MakeTree());
        state.SelectedChoice = 1;
        state.GoToNode("node_b");

        Assert.Equal(0, state.SelectedChoice);
    }
}
