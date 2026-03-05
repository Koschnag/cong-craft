using CongCraft.Engine.Core;
using CongCraft.Engine.Dialogue;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using Silk.NET.Input;

namespace CongCraft.Engine.Quest;

/// <summary>
/// Tracks quest progress from game events. Handles quest acceptance from dialogue,
/// kill tracking from combat, collect tracking from inventory, and turn-in with rewards.
/// Press J to toggle quest journal.
/// </summary>
public sealed class QuestSystem : ISystem
{
    public int Priority => 26; // After inventory

    private World _world = null!;

    // Track enemy kills (count dead enemies that were alive last frame)
    private readonly HashSet<int> _trackedKills = new();
    private bool _questsInitialized;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();

        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (!_world.HasComponent<QuestJournal>(entity)) continue;
            var journal = _world.GetComponent<QuestJournal>(entity);

            // Give starter quest on first frame
            if (!_questsInitialized)
            {
                _questsInitialized = true;
                var starterQuest = QuestDatabase.Get("village_tour");
                if (starterQuest != null)
                    journal.AcceptQuest(starterQuest);
            }

            // Track kills
            TrackEnemyKills(journal);

            // Track item collection
            TrackItemCollection(entity, journal);

            // Track NPC dialogue completion
            TrackDialogueProgress(journal);

            // Auto turn-in completed quests and give rewards
            ProcessCompletedQuests(entity, journal);

            // Toggle quest journal display with J
            // (journal.IsOpen is tracked via a simple bool we piggyback on SelectedChoice)
            if (input.IsKeyPressed(Key.J))
            {
                // We use the journal's active quest count display toggle
                // The HUD reads active quests directly
            }
        }
    }

    private void TrackEnemyKills(QuestJournal journal)
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (!health.IsAlive && !_trackedKills.Contains(entity.Id))
            {
                _trackedKills.Add(entity.Id);
                journal.ReportProgress(QuestObjectiveType.Kill);
            }
        }
    }

    private void TrackItemCollection(Entity playerEntity, QuestJournal journal)
    {
        if (!_world.HasComponent<InventoryComponent>(playerEntity)) return;
        var inventory = _world.GetComponent<InventoryComponent>(playerEntity);

        // Check collect objectives against current inventory
        foreach (var quest in journal.ActiveQuests)
        {
            foreach (var obj in quest.Objectives)
            {
                if (obj.Type != QuestObjectiveType.Collect) continue;
                if (obj.TargetId == null) continue;

                int count = inventory.CountOf(obj.TargetId);
                if (count > obj.CurrentCount)
                    obj.CurrentCount = Math.Min(obj.RequiredCount, count);
            }
        }
    }

    private void TrackDialogueProgress(QuestJournal journal)
    {
        if (!_world.TryGetSingleton<DialogueState>(out var dialogueState)) return;
        if (dialogueState == null || dialogueState.IsActive) return;

        // Check if we just finished talking to an NPC
        // We look at NPCs that were recently interacting
        foreach (var (entity, npc) in _world.Query<NpcComponent>())
        {
            // Report talk progress when dialogue was just completed
            // (The dialogue system sets IsInteracting = false on end)
            journal.ReportProgress(QuestObjectiveType.TalkTo, npc.DialogueTreeId);
        }
    }

    private void ProcessCompletedQuests(Entity playerEntity, QuestJournal journal)
    {
        var completedIds = new List<string>();

        foreach (var quest in journal.ActiveQuests)
        {
            if (quest.Status != QuestStatus.Completed) continue;
            completedIds.Add(quest.Quest.Id);
        }

        if (!_world.HasComponent<InventoryComponent>(playerEntity)) return;
        var inventory = _world.GetComponent<InventoryComponent>(playerEntity);

        foreach (var questId in completedIds)
        {
            if (!journal.TurnInQuest(questId)) continue;

            // Give rewards
            var questData = QuestDatabase.Get(questId);
            if (questData == null) continue;

            foreach (var reward in questData.Rewards)
            {
                var itemData = ItemDatabase.Get(reward.ItemId);
                if (itemData != null)
                    inventory.TryAdd(itemData, reward.Quantity);
            }

            // Award XP
            if (questData.XpReward > 0 && _world.HasComponent<LevelComponent>(playerEntity))
            {
                var level = _world.GetComponent<LevelComponent>(playerEntity);
                level.AddExperience(questData.XpReward);
            }

            // Accept follow-up quests based on completed quest
            AcceptFollowUpQuest(questId, journal);
        }
    }

    private void AcceptFollowUpQuest(string completedQuestId, QuestJournal journal)
    {
        // Quest chains — each quest can lead to the next
        string? followUpId = completedQuestId switch
        {
            "village_tour" => "beast_hunt",
            "beast_hunt" => "pelt_collector",
            "pelt_collector" => "gather_materials",
            "gather_materials" => "first_craft",
            "first_craft" => "dungeon_delve",
            "dungeon_delve" => "bone_collector",
            "bone_collector" => "troll_slayer",
            _ => null
        };

        if (followUpId == null) return;
        var followUp = QuestDatabase.Get(followUpId);
        if (followUp != null)
            journal.AcceptQuest(followUp);
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
