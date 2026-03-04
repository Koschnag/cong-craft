using CongCraft.Engine.SaveLoad;

namespace CongCraft.Engine.Tests.SaveLoad;

public class SaveDataTests
{
    [Fact]
    public void SaveData_DefaultValues()
    {
        var save = new SaveData();
        Assert.Equal("1.0", save.Version);
        Assert.NotNull(save.Player);
        Assert.Empty(save.ActiveQuests);
        Assert.Empty(save.CompletedQuestIds);
        Assert.Empty(save.Inventory);
        Assert.NotNull(save.Equipment);
    }

    [Fact]
    public void PlayerSaveData_DefaultValues()
    {
        var player = new PlayerSaveData();
        Assert.Equal(100f, player.Health);
        Assert.Equal(1, player.Level);
        Assert.Equal(0, player.Experience);
    }

    [Fact]
    public void EquipmentSaveData_DefaultsNull()
    {
        var equip = new EquipmentSaveData();
        Assert.Null(equip.MainHandId);
        Assert.Null(equip.HeadId);
        Assert.Null(equip.ChestId);
        Assert.Null(equip.LegsId);
    }
}
