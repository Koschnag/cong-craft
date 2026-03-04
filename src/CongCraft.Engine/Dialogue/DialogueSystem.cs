using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Inventory;
using Silk.NET.Input;

namespace CongCraft.Engine.Dialogue;

/// <summary>
/// Handles NPC interaction, dialogue node traversal, and choice selection.
/// Press T to talk to nearby NPC, Enter to advance/select, Up/Down for choices, Escape to close.
/// </summary>
public sealed class DialogueSystem : ISystem
{
    public int Priority => 8; // Before movement (blocks movement during dialogue)

    private World _world = null!;
    private DialogueState _dialogueState = null!;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _dialogueState = new DialogueState();
        _world.SetSingleton(_dialogueState);
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();

        if (_dialogueState.IsActive)
        {
            UpdateActiveDialogue(input);
        }
        else
        {
            // Check for interaction with nearby NPC
            if (input.IsKeyPressed(Key.T))
                TryStartDialogue();
        }
    }

    private void TryStartDialogue()
    {
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        // Find closest NPC in range
        Entity closestNpc = default;
        float closestDist = float.MaxValue;
        NpcComponent? closestNpcComp = null;

        foreach (var (entity, npc, transform) in _world.Query<NpcComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist < npc.InteractionRadius && dist < closestDist)
            {
                closestDist = dist;
                closestNpc = entity;
                closestNpcComp = npc;
            }
        }

        if (closestNpcComp == null) return;

        var tree = DialogueDatabase.Get(closestNpcComp.DialogueTreeId);
        if (tree == null) return;

        closestNpcComp.IsInteracting = true;
        _dialogueState.Start(closestNpc, tree);
    }

    private void UpdateActiveDialogue(InputState input)
    {
        var node = _dialogueState.CurrentNode;
        if (node == null)
        {
            EndDialogue();
            return;
        }

        // Close dialogue with Escape
        if (input.IsKeyPressed(Key.Escape))
        {
            EndDialogue();
            return;
        }

        if (node.Choices.Count > 0)
        {
            // Navigate choices
            if (input.IsKeyPressed(Key.Up) && _dialogueState.SelectedChoice > 0)
                _dialogueState.SelectedChoice--;
            if (input.IsKeyPressed(Key.Down) && _dialogueState.SelectedChoice < node.Choices.Count - 1)
                _dialogueState.SelectedChoice++;

            // Select choice with Enter
            if (input.IsKeyPressed(Key.Enter))
            {
                var choice = node.Choices[_dialogueState.SelectedChoice];

                // Check requirements and process rewards
                if (ProcessChoice(choice))
                    _dialogueState.GoToNode(choice.NextNodeId);
            }
        }
        else
        {
            // No choices: advance or end with Enter
            if (input.IsKeyPressed(Key.Enter))
            {
                if (node.NextNodeId != null)
                    _dialogueState.GoToNode(node.NextNodeId);
                else
                    EndDialogue();
            }
        }
    }

    private bool ProcessChoice(DialogueChoice choice)
    {
        // Find player inventory for requirement/reward processing
        InventoryComponent? inventory = null;
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (_world.HasComponent<InventoryComponent>(entity))
                inventory = _world.GetComponent<InventoryComponent>(entity);
            break;
        }

        // Check item requirement
        if (choice.RequiredItemId != null && inventory != null)
        {
            int required = choice.RequiredItemId == "gold_coin" ? 5 : 3; // Trade costs
            if (inventory.CountOf(choice.RequiredItemId) < required)
                return false; // Can't afford

            inventory.TryRemove(choice.RequiredItemId, required);
        }

        // Give reward
        if (choice.RewardItemId != null && inventory != null)
        {
            var rewardItem = ItemDatabase.Get(choice.RewardItemId);
            if (rewardItem != null)
                inventory.TryAdd(rewardItem, choice.RewardQuantity);
        }

        return true;
    }

    private void EndDialogue()
    {
        // Reset NPC interaction state
        if (_world.HasEntity(_dialogueState.InteractingNpc) &&
            _world.HasComponent<NpcComponent>(_dialogueState.InteractingNpc))
        {
            _world.GetComponent<NpcComponent>(_dialogueState.InteractingNpc).IsInteracting = false;
        }

        _dialogueState.End();
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
