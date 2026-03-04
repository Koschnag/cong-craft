using CongCraft.Engine.SaveLoad;

namespace CongCraft.Engine.Tests.SaveLoad;

public class SaveSerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_RoundTrip()
    {
        var save = new SaveData
        {
            Player = new PlayerSaveData
            {
                PositionX = 10.5f,
                PositionY = 5.2f,
                PositionZ = -3.1f,
                Health = 75f,
                MaxHealth = 120f,
                Mana = 30f,
                Level = 5,
                Experience = 200,
                SkillPoints = 3,
                Strength = 4,
                Endurance = 2,
                Agility = 3
            },
            Inventory =
            {
                new InventoryItemSave { ItemId = "iron_sword", Quantity = 1 },
                new InventoryItemSave { ItemId = "health_potion", Quantity = 5 },
                new InventoryItemSave { ItemId = "gold_coin", Quantity = 42 }
            },
            Equipment = new EquipmentSaveData
            {
                MainHandId = "iron_sword",
                ChestId = "chain_mail"
            },
            ActiveQuests =
            {
                new QuestSaveData
                {
                    QuestId = "beast_hunt",
                    Objectives =
                    {
                        new QuestObjectiveSave { CurrentCount = 3, IsComplete = false }
                    }
                }
            },
            CompletedQuestIds = { "village_tour" }
        };

        string json = SaveSerializer.SerializeToString(save);
        var loaded = SaveSerializer.DeserializeFromString(json);

        Assert.NotNull(loaded);
        Assert.Equal(10.5f, loaded.Player.PositionX);
        Assert.Equal(75f, loaded.Player.Health);
        Assert.Equal(5, loaded.Player.Level);
        Assert.Equal(4, loaded.Player.Strength);
        Assert.Equal(3, loaded.Inventory.Count);
        Assert.Equal("iron_sword", loaded.Inventory[0].ItemId);
        Assert.Equal(42, loaded.Inventory[2].Quantity);
        Assert.Equal("iron_sword", loaded.Equipment.MainHandId);
        Assert.Equal("chain_mail", loaded.Equipment.ChestId);
        Assert.Null(loaded.Equipment.HeadId);
        Assert.Single(loaded.ActiveQuests);
        Assert.Equal("beast_hunt", loaded.ActiveQuests[0].QuestId);
        Assert.Single(loaded.CompletedQuestIds);
    }

    [Fact]
    public void SerializeToString_ProducesValidJson()
    {
        var save = new SaveData();
        string json = SaveSerializer.SerializeToString(save);
        Assert.Contains("\"version\"", json);
        Assert.Contains("\"player\"", json);
    }

    [Fact]
    public void DeserializeFromString_ReturnsNullForInvalidJson()
    {
        // Invalid JSON should throw or return null
        Assert.ThrowsAny<Exception>(() => SaveSerializer.DeserializeFromString("not json"));
    }

    [Fact]
    public void EmptySaveData_RoundTrips()
    {
        var save = new SaveData();
        string json = SaveSerializer.SerializeToString(save);
        var loaded = SaveSerializer.DeserializeFromString(json);

        Assert.NotNull(loaded);
        Assert.Equal("1.0", loaded.Version);
        Assert.Empty(loaded.Inventory);
        Assert.Empty(loaded.ActiveQuests);
    }
}
