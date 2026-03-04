namespace CongCraft.Engine.Quest;

/// <summary>
/// Registry of all quest definitions.
/// </summary>
public static class QuestDatabase
{
    private static readonly Dictionary<string, QuestData> Quests = new();

    static QuestDatabase()
    {
        Register(new QuestData
        {
            Id = "beast_hunt",
            Title = "Beast Hunt",
            Description = "The Elder asks you to slay the beasts threatening the village.",
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

        Register(new QuestData
        {
            Id = "village_tour",
            Title = "Welcome to the Village",
            Description = "Speak to the villagers and learn about the situation.",
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
    }

    private static void Register(QuestData quest) => Quests[quest.Id] = quest;

    public static QuestData? Get(string id) => Quests.GetValueOrDefault(id);
    public static IReadOnlyDictionary<string, QuestData> All => Quests;
}
