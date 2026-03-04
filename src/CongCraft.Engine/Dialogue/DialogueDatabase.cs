namespace CongCraft.Engine.Dialogue;

/// <summary>
/// Registry of all dialogue trees in the game.
/// </summary>
public static class DialogueDatabase
{
    private static readonly Dictionary<string, DialogueTree> Trees = new();

    static DialogueDatabase()
    {
        // Blacksmith NPC
        Register(new DialogueTree { Id = "blacksmith", StartNodeId = "greeting" }
            .AddNode(new DialogueNode
            {
                Id = "greeting", SpeakerName = "Blacksmith Aldric",
                Text = "The forge burns day and night. What brings you here, traveler?",
                Choices = new()
                {
                    new() { Text = "I need a better weapon.", NextNodeId = "weapon_talk" },
                    new() { Text = "What is this place?", NextNodeId = "about_town" },
                    new() { Text = "Farewell.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "weapon_talk", SpeakerName = "Blacksmith Aldric",
                Text = "Iron is scarce since the mines fell to darkness. Bring me wolf pelts and I can trade you a fine blade.",
                Choices = new()
                {
                    new()
                    {
                        Text = "Here, take these pelts. [Trade 3 Wolf Pelts]",
                        NextNodeId = "trade_success",
                        RequiredItemId = "wolf_pelt",
                        RewardItemId = "iron_sword",
                        RewardQuantity = 1
                    },
                    new() { Text = "I will return when I have them.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "trade_success", SpeakerName = "Blacksmith Aldric",
                Text = "Fine pelts! Here, take this blade. Forged it myself. May it serve you well against the darkness.",
                NextNodeId = null
            })
            .AddNode(new DialogueNode
            {
                Id = "about_town", SpeakerName = "Blacksmith Aldric",
                Text = "This was once a prosperous village. Now shadows creep from the east. The guards are spread thin and beasts roam closer each night.",
                Choices = new()
                {
                    new() { Text = "I will help drive them back.", NextNodeId = "help_offer" },
                    new() { Text = "Farewell.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "help_offer", SpeakerName = "Blacksmith Aldric",
                Text = "A brave soul! Speak to the Elder near the well. She knows more about the threat than any of us.",
                NextNodeId = null
            })
            .AddNode(new DialogueNode
            {
                Id = "goodbye", SpeakerName = "Blacksmith Aldric",
                Text = "Safe travels, friend.",
                NextNodeId = null
            }));

        // Elder NPC
        Register(new DialogueTree { Id = "elder", StartNodeId = "greeting" }
            .AddNode(new DialogueNode
            {
                Id = "greeting", SpeakerName = "Elder Maren",
                Text = "Ah, a wanderer. These are dark times. The creatures from the eastern woods grow bolder with each passing moon.",
                Choices = new()
                {
                    new() { Text = "What happened here?", NextNodeId = "story" },
                    new() { Text = "How can I help?", NextNodeId = "quest_offer" },
                    new() { Text = "I must go.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "story", SpeakerName = "Elder Maren",
                Text = "An ancient evil stirs beneath the mountains. Our scouts never returned from the caves. Something woke down there... something old.",
                NextNodeId = "greeting"
            })
            .AddNode(new DialogueNode
            {
                Id = "quest_offer", SpeakerName = "Elder Maren",
                Text = "The beasts must be culled. Slay them and bring proof of your deed. I can reward you with knowledge of the dark blade hidden in the old shrine.",
                Choices = new()
                {
                    new() { Text = "Consider it done.", NextNodeId = "quest_accepted" },
                    new() { Text = "I need to prepare first.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "quest_accepted", SpeakerName = "Elder Maren",
                Text = "May the old gods watch over you. Return when the deed is done.",
                NextNodeId = null
            })
            .AddNode(new DialogueNode
            {
                Id = "goodbye", SpeakerName = "Elder Maren",
                Text = "Be careful out there. The night holds many dangers.",
                NextNodeId = null
            }));

        // Merchant NPC
        Register(new DialogueTree { Id = "merchant", StartNodeId = "greeting" }
            .AddNode(new DialogueNode
            {
                Id = "greeting", SpeakerName = "Merchant Gregor",
                Text = "Wares for sale! Potions, supplies, everything a traveler needs. What catches your eye?",
                Choices = new()
                {
                    new()
                    {
                        Text = "Buy a Health Potion. [5 Gold]",
                        NextNodeId = "bought_potion",
                        RequiredItemId = "gold_coin",
                        RewardItemId = "health_potion",
                        RewardQuantity = 1
                    },
                    new() { Text = "What news from the road?", NextNodeId = "news" },
                    new() { Text = "Nothing for now.", NextNodeId = "goodbye" }
                }
            })
            .AddNode(new DialogueNode
            {
                Id = "bought_potion", SpeakerName = "Merchant Gregor",
                Text = "A wise purchase. One can never have too many potions in these troubled times.",
                NextNodeId = "greeting"
            })
            .AddNode(new DialogueNode
            {
                Id = "news", SpeakerName = "Merchant Gregor",
                Text = "Bandits on the north road, wolves in the eastern forest, and strange lights from the mountains at night. Business has never been worse... or better, depending on how you look at it.",
                NextNodeId = "greeting"
            })
            .AddNode(new DialogueNode
            {
                Id = "goodbye", SpeakerName = "Merchant Gregor",
                Text = "Come back anytime! Gold is always welcome.",
                NextNodeId = null
            }));
    }

    private static void Register(DialogueTree tree) => Trees[tree.Id] = tree;

    public static DialogueTree? Get(string id) => Trees.GetValueOrDefault(id);
    public static IReadOnlyDictionary<string, DialogueTree> All => Trees;
}
