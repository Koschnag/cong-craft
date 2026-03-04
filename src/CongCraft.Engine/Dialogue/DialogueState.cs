using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Dialogue;

/// <summary>
/// Singleton component tracking active dialogue state.
/// </summary>
public sealed class DialogueState : IComponent
{
    public bool IsActive { get; set; }
    public Entity InteractingNpc { get; set; }
    public DialogueTree? CurrentTree { get; set; }
    public DialogueNode? CurrentNode { get; set; }
    public int SelectedChoice { get; set; }

    public void Start(Entity npc, DialogueTree tree)
    {
        IsActive = true;
        InteractingNpc = npc;
        CurrentTree = tree;
        CurrentNode = tree.StartNode;
        SelectedChoice = 0;
    }

    public void End()
    {
        IsActive = false;
        CurrentTree = null;
        CurrentNode = null;
        SelectedChoice = 0;
    }

    public void GoToNode(string nodeId)
    {
        if (CurrentTree == null) return;
        CurrentNode = CurrentTree.GetNode(nodeId);
        SelectedChoice = 0;

        if (CurrentNode == null)
            End();
    }
}
