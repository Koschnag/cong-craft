using CongCraft.Engine.Magic;

namespace CongCraft.Engine.Tests.Magic;

public class ManaComponentTests
{
    [Fact]
    public void DefaultValues()
    {
        var mana = new ManaComponent();
        Assert.Equal(50f, mana.Current);
        Assert.Equal(50f, mana.Max);
        Assert.Equal(2f, mana.RegenRate);
        Assert.Equal(1f, mana.Percentage);
    }

    [Fact]
    public void TrySpend_SucceedsWithEnoughMana()
    {
        var mana = new ManaComponent { Current = 30f };
        Assert.True(mana.TrySpend(15f));
        Assert.Equal(15f, mana.Current);
    }

    [Fact]
    public void TrySpend_FailsWithInsufficientMana()
    {
        var mana = new ManaComponent { Current = 10f };
        Assert.False(mana.TrySpend(15f));
        Assert.Equal(10f, mana.Current); // Unchanged
    }

    [Fact]
    public void CanSpend_ChecksAvailability()
    {
        var mana = new ManaComponent { Current = 20f };
        Assert.True(mana.CanSpend(20f));
        Assert.True(mana.CanSpend(10f));
        Assert.False(mana.CanSpend(25f));
    }

    [Fact]
    public void Regenerate_RestoresMana()
    {
        var mana = new ManaComponent { Current = 30f, Max = 50f, RegenRate = 5f };
        mana.Regenerate(2f); // 5 * 2 = +10
        Assert.Equal(40f, mana.Current);
    }

    [Fact]
    public void Regenerate_CapsAtMax()
    {
        var mana = new ManaComponent { Current = 48f, Max = 50f, RegenRate = 10f };
        mana.Regenerate(1f); // Would add 10, but capped
        Assert.Equal(50f, mana.Current);
    }

    [Fact]
    public void Percentage_CalculatesCorrectly()
    {
        var mana = new ManaComponent { Current = 25f, Max = 50f };
        Assert.Equal(0.5f, mana.Percentage);
    }
}
