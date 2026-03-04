using System.Text.Json.Serialization;

namespace CongCraft.Engine.SaveLoad;

/// <summary>
/// Root save data structure containing all serializable game state.
/// </summary>
public sealed class SaveData
{
    public string Version { get; set; } = "1.0";
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public PlayerSaveData Player { get; set; } = new();
    public List<QuestSaveData> ActiveQuests { get; set; } = new();
    public List<string> CompletedQuestIds { get; set; } = new();
    public List<InventoryItemSave> Inventory { get; set; } = new();
    public EquipmentSaveData Equipment { get; set; } = new();
}

public sealed class PlayerSaveData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Mana { get; set; } = 50f;
    public float MaxMana { get; set; } = 50f;
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int SkillPoints { get; set; }
    public int Strength { get; set; }
    public int Endurance { get; set; }
    public int Agility { get; set; }
}

public sealed class QuestSaveData
{
    public string QuestId { get; set; } = "";
    public List<QuestObjectiveSave> Objectives { get; set; } = new();
}

public sealed class QuestObjectiveSave
{
    public int CurrentCount { get; set; }
    public bool IsComplete { get; set; }
}

public sealed class InventoryItemSave
{
    public string ItemId { get; set; } = "";
    public int Quantity { get; set; } = 1;
}

public sealed class EquipmentSaveData
{
    public string? MainHandId { get; set; }
    public string? HeadId { get; set; }
    public string? ChestId { get; set; }
    public string? LegsId { get; set; }
}
