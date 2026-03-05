namespace CongCraft.Engine.Quest;

/// <summary>
/// Registry of all quest definitions.
/// </summary>
public static class QuestDatabase
{
    private static readonly Dictionary<string, QuestData> Quests = new();

    static QuestDatabase()
    {
        // === MAIN QUEST CHAIN ===

        Register(new QuestData
        {
            Id = "village_tour",
            Title = "Welcome to the Village",
            Description = "Speak to the villagers and learn about the situation.",
            XpReward = 50,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "talk_blacksmith",
                    Description = "Speak with the Blacksmith",
                    Type = QuestObjectiveType.TalkTo,
                    TargetId = "blacksmith"
                },
                new QuestObjective
                {
                    Id = "talk_merchant",
                    Description = "Speak with the Merchant",
                    Type = QuestObjectiveType.TalkTo,
                    TargetId = "merchant"
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "health_potion", Quantity = 3 },
                new QuestReward { ItemId = "gold_coin", Quantity = 5 }
            }
        });

        Register(new QuestData
        {
            Id = "beast_hunt",
            Title = "Beast Hunt",
            Description = "The Elder asks you to slay the beasts threatening the village.",
            XpReward = 100,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "kill_enemies",
                    Description = "Slay hostile creatures",
                    Type = QuestObjectiveType.Kill,
                    RequiredCount = 5
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "dark_blade", Quantity = 1 },
                new QuestReward { ItemId = "gold_coin", Quantity = 10 }
            }
        });

        Register(new QuestData
        {
            Id = "pelt_collector",
            Title = "Pelt Collector",
            Description = "The Blacksmith needs wolf pelts for his forge work.",
            XpReward = 75,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "collect_pelts",
                    Description = "Collect wolf pelts from fallen beasts",
                    Type = QuestObjectiveType.Collect,
                    TargetId = "wolf_pelt",
                    RequiredCount = 3
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "iron_sword", Quantity = 1 },
                new QuestReward { ItemId = "health_potion", Quantity = 2 }
            }
        });

        // === CRAFTING QUEST CHAIN ===

        Register(new QuestData
        {
            Id = "gather_materials",
            Title = "Gathering Supplies",
            Description = "Collect raw materials for the Blacksmith's workshop.",
            XpReward = 60,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "collect_iron",
                    Description = "Collect iron ore",
                    Type = QuestObjectiveType.Collect,
                    TargetId = "iron_ore",
                    RequiredCount = 5
                },
                new QuestObjective
                {
                    Id = "collect_leather",
                    Description = "Collect leather",
                    Type = QuestObjectiveType.Collect,
                    TargetId = "leather",
                    RequiredCount = 3
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "gold_coin", Quantity = 15 },
                new QuestReward { ItemId = "strength_elixir", Quantity = 1 }
            }
        });

        Register(new QuestData
        {
            Id = "first_craft",
            Title = "Apprentice Smith",
            Description = "Use the Anvil to forge your first piece of armor.",
            XpReward = 80,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "collect_for_craft",
                    Description = "Collect iron ore for crafting",
                    Type = QuestObjectiveType.Collect,
                    TargetId = "iron_ore",
                    RequiredCount = 3
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "chain_mail", Quantity = 1 },
                new QuestReward { ItemId = "gold_coin", Quantity = 8 }
            }
        });

        // === DUNGEON QUEST ===

        Register(new QuestData
        {
            Id = "dungeon_delve",
            Title = "Into the Depths",
            Description = "Brave the dungeon and clear out the monsters lurking within.",
            XpReward = 150,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "kill_dungeon_enemies",
                    Description = "Slay dungeon creatures",
                    Type = QuestObjectiveType.Kill,
                    RequiredCount = 8
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "dark_blade", Quantity = 1 },
                new QuestReward { ItemId = "health_potion", Quantity = 5 },
                new QuestReward { ItemId = "mana_potion", Quantity = 3 },
                new QuestReward { ItemId = "gold_coin", Quantity = 25 }
            }
        });

        // === BONE COLLECTOR ===

        Register(new QuestData
        {
            Id = "bone_collector",
            Title = "Bone Collector",
            Description = "The Alchemist needs skeleton bones for her experiments.",
            XpReward = 90,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "collect_bones",
                    Description = "Collect bones from skeleton remains",
                    Type = QuestObjectiveType.Collect,
                    TargetId = "bone",
                    RequiredCount = 5
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "mana_potion", Quantity = 3 },
                new QuestReward { ItemId = "strength_elixir", Quantity = 2 },
                new QuestReward { ItemId = "gold_coin", Quantity = 12 }
            }
        });

        // === TROLL SLAYER ===

        Register(new QuestData
        {
            Id = "troll_slayer",
            Title = "Troll Slayer",
            Description = "A fearsome Troll has been sighted in the eastern hills. Slay it to prove your valor.",
            XpReward = 200,
            Objectives = new()
            {
                new QuestObjective
                {
                    Id = "kill_troll",
                    Description = "Slay the Troll",
                    Type = QuestObjectiveType.Kill,
                    RequiredCount = 1
                }
            },
            Rewards = new()
            {
                new QuestReward { ItemId = "wolf_cloak", Quantity = 1 },
                new QuestReward { ItemId = "gold_coin", Quantity = 30 },
                new QuestReward { ItemId = "health_potion", Quantity = 3 }
            }
        });
    }

    private static void Register(QuestData quest) => Quests[quest.Id] = quest;

    public static QuestData? Get(string id) => Quests.GetValueOrDefault(id);
    public static IReadOnlyDictionary<string, QuestData> All => Quests;
}
