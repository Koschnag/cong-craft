using CongCraft.Engine.Dialogue;

namespace CongCraft.Engine.Tests.Dialogue;

public class DialogueDatabaseTests
{
    [Fact]
    public void Get_ReturnsBlacksmith()
    {
        var tree = DialogueDatabase.Get("blacksmith");
        Assert.NotNull(tree);
        Assert.NotNull(tree.StartNode);
        Assert.Equal("Blacksmith Aldric", tree.StartNode!.SpeakerName);
    }

    [Fact]
    public void Get_ReturnsElder()
    {
        var tree = DialogueDatabase.Get("elder");
        Assert.NotNull(tree);
        Assert.NotNull(tree.StartNode);
        Assert.Equal("Elder Maren", tree.StartNode!.SpeakerName);
    }

    [Fact]
    public void Get_ReturnsMerchant()
    {
        var tree = DialogueDatabase.Get("merchant");
        Assert.NotNull(tree);
        Assert.NotNull(tree.StartNode);
    }

    [Fact]
    public void Get_ReturnsNullForUnknown()
    {
        Assert.Null(DialogueDatabase.Get("nonexistent"));
    }

    [Fact]
    public void All_HasThreeTrees()
    {
        Assert.Equal(3, DialogueDatabase.All.Count);
    }

    [Fact]
    public void BlacksmithTree_HasChoicesOnGreeting()
    {
        var tree = DialogueDatabase.Get("blacksmith")!;
        var greeting = tree.StartNode!;
        Assert.True(greeting.Choices.Count >= 2, "Blacksmith greeting should have choices");
    }

    [Fact]
    public void BlacksmithTree_TradeChoice_HasRequirement()
    {
        var tree = DialogueDatabase.Get("blacksmith")!;
        var weaponTalk = tree.GetNode("weapon_talk");
        Assert.NotNull(weaponTalk);

        var tradeChoice = weaponTalk!.Choices.FirstOrDefault(c => c.RequiredItemId != null);
        Assert.NotNull(tradeChoice);
        Assert.Equal("wolf_pelt", tradeChoice!.RequiredItemId);
        Assert.Equal("iron_sword", tradeChoice.RewardItemId);
    }
}
