using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using CongCraft.Engine.Magic;
using CongCraft.Engine.Quest;
using Silk.NET.Input;

namespace CongCraft.Engine.SaveLoad;

/// <summary>
/// Handles save (F5) and load (F9) of game state.
/// </summary>
public sealed class SaveLoadSystem : ISystem
{
    public int Priority => 29; // After most gameplay systems

    private World _world = null!;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _world.SetSingleton(new SaveNotification());
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;
        var notif = _world.GetSingleton<SaveNotification>();

        if (notif.Timer > 0)
            notif.Timer -= dt;

        if (input.IsKeyPressed(Key.F5))
        {
            var data = ExtractSaveData();
            SaveSerializer.Save(data);
            ShowNotification("Game Saved");
        }

        if (input.IsKeyPressed(Key.F9))
        {
            var data = SaveSerializer.Load();
            if (data != null)
            {
                ApplySaveData(data);
                ShowNotification("Game Loaded");
            }
            else
            {
                ShowNotification("No Save Found");
            }
        }
    }

    private void ShowNotification(string text)
    {
        var notif = _world.GetSingleton<SaveNotification>();
        notif.Text = text;
        notif.Timer = 2f;
    }

    /// <summary>
    /// Extracts current game state into a SaveData structure.
    /// </summary>
    public SaveData ExtractSaveData()
    {
        var save = new SaveData();

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            // Position
            save.Player.PositionX = transform.Position.X;
            save.Player.PositionY = transform.Position.Y;
            save.Player.PositionZ = transform.Position.Z;

            // Health
            if (_world.HasComponent<HealthComponent>(entity))
            {
                var health = _world.GetComponent<HealthComponent>(entity);
                save.Player.Health = health.Current;
                save.Player.MaxHealth = health.Max;
            }

            // Mana
            if (_world.HasComponent<ManaComponent>(entity))
            {
                var mana = _world.GetComponent<ManaComponent>(entity);
                save.Player.Mana = mana.Current;
                save.Player.MaxMana = mana.Max;
            }

            // Level & Skills
            if (_world.HasComponent<LevelComponent>(entity))
            {
                var level = _world.GetComponent<LevelComponent>(entity);
                save.Player.Level = level.Level;
                save.Player.Experience = level.Experience;
                save.Player.SkillPoints = level.SkillPoints;
            }

            if (_world.HasComponent<SkillTree>(entity))
            {
                var skills = _world.GetComponent<SkillTree>(entity);
                save.Player.Strength = skills.Strength;
                save.Player.Endurance = skills.Endurance;
                save.Player.Agility = skills.Agility;
            }

            // Inventory
            if (_world.HasComponent<InventoryComponent>(entity))
            {
                var inv = _world.GetComponent<InventoryComponent>(entity);
                foreach (var stack in inv.Items)
                {
                    save.Inventory.Add(new InventoryItemSave
                    {
                        ItemId = stack.Item.Id,
                        Quantity = stack.Quantity
                    });
                }
            }

            // Equipment
            if (_world.HasComponent<EquipmentComponent>(entity))
            {
                var equip = _world.GetComponent<EquipmentComponent>(entity);
                save.Equipment.MainHandId = equip.MainHand?.Item.Id;
                save.Equipment.HeadId = equip.Head?.Item.Id;
                save.Equipment.ChestId = equip.Chest?.Item.Id;
                save.Equipment.LegsId = equip.Legs?.Item.Id;
            }

            // Quests
            if (_world.HasComponent<QuestJournal>(entity))
            {
                var journal = _world.GetComponent<QuestJournal>(entity);
                foreach (var quest in journal.ActiveQuests)
                {
                    var qs = new QuestSaveData { QuestId = quest.Quest.Id };
                    foreach (var obj in quest.Objectives)
                    {
                        qs.Objectives.Add(new QuestObjectiveSave
                        {
                            CurrentCount = obj.CurrentCount,
                            IsComplete = obj.IsComplete
                        });
                    }
                    save.ActiveQuests.Add(qs);
                }
                foreach (var quest in journal.CompletedQuests)
                    save.CompletedQuestIds.Add(quest.Quest.Id);
            }

            break; // Only first player
        }

        return save;
    }

    /// <summary>
    /// Applies saved state back to the game world.
    /// </summary>
    public void ApplySaveData(SaveData save)
    {
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            // Position
            transform.Position = new Vector3(save.Player.PositionX, save.Player.PositionY, save.Player.PositionZ);

            // Health
            if (_world.HasComponent<HealthComponent>(entity))
            {
                var health = _world.GetComponent<HealthComponent>(entity);
                health.Current = save.Player.Health;
                health.Max = save.Player.MaxHealth;
            }

            // Mana
            if (_world.HasComponent<ManaComponent>(entity))
            {
                var mana = _world.GetComponent<ManaComponent>(entity);
                mana.Current = save.Player.Mana;
                mana.Max = save.Player.MaxMana;
            }

            // Level & Skills
            if (_world.HasComponent<LevelComponent>(entity))
            {
                var level = _world.GetComponent<LevelComponent>(entity);
                level.Level = save.Player.Level;
                level.Experience = save.Player.Experience;
                level.SkillPoints = save.Player.SkillPoints;
            }

            if (_world.HasComponent<SkillTree>(entity))
            {
                var skills = _world.GetComponent<SkillTree>(entity);
                skills.Strength = save.Player.Strength;
                skills.Endurance = save.Player.Endurance;
                skills.Agility = save.Player.Agility;
            }

            // Inventory
            if (_world.HasComponent<InventoryComponent>(entity))
            {
                var inv = _world.GetComponent<InventoryComponent>(entity);
                inv.Items.Clear();
                foreach (var itemSave in save.Inventory)
                {
                    var itemData = ItemDatabase.Get(itemSave.ItemId);
                    if (itemData != null)
                        inv.TryAdd(itemData, itemSave.Quantity);
                }
            }

            // Equipment
            if (_world.HasComponent<EquipmentComponent>(entity))
            {
                var equip = _world.GetComponent<EquipmentComponent>(entity);
                equip.MainHand = CreateStack(save.Equipment.MainHandId);
                equip.Head = CreateStack(save.Equipment.HeadId);
                equip.Chest = CreateStack(save.Equipment.ChestId);
                equip.Legs = CreateStack(save.Equipment.LegsId);
            }

            // Quests
            if (_world.HasComponent<QuestJournal>(entity))
            {
                var journal = _world.GetComponent<QuestJournal>(entity);
                journal.ActiveQuests.Clear();
                journal.CompletedQuests.Clear();

                foreach (var qs in save.ActiveQuests)
                {
                    var questData = QuestDatabase.Get(qs.QuestId);
                    if (questData == null) continue;

                    journal.AcceptQuest(questData);
                    var active = journal.GetActiveQuest(qs.QuestId);
                    if (active == null) continue;

                    for (int i = 0; i < active.Objectives.Count && i < qs.Objectives.Count; i++)
                    {
                        active.Objectives[i].CurrentCount = qs.Objectives[i].CurrentCount;
                    }
                }

                // Mark completed quests
                foreach (var completedId in save.CompletedQuestIds)
                {
                    var questData = QuestDatabase.Get(completedId);
                    if (questData == null) continue;

                    journal.AcceptQuest(questData);
                    var active = journal.GetActiveQuest(completedId);
                    if (active != null)
                    {
                        // Complete all objectives
                        foreach (var obj in active.Objectives)
                            obj.CurrentCount = obj.RequiredCount;
                        journal.TurnInQuest(completedId);
                    }
                }
            }

            break; // Only first player
        }
    }

    private static ItemStack? CreateStack(string? itemId)
    {
        if (itemId == null) return null;
        var item = ItemDatabase.Get(itemId);
        return item != null ? new ItemStack { Item = item, Quantity = 1 } : null;
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
