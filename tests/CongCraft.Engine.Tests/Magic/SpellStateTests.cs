using CongCraft.Engine.Magic;

namespace CongCraft.Engine.Tests.Magic;

public class SpellStateTests
{
    [Fact]
    public void DefaultValues()
    {
        var state = new SpellState();
        Assert.Equal(0, state.SelectedSpell);
        Assert.Equal(4, state.Cooldowns.Length);
        Assert.All(state.Cooldowns, cd => Assert.Equal(0f, cd));
        Assert.False(state.HasActiveShield);
    }

    [Fact]
    public void HasActiveShield_WhenTimerPositive()
    {
        var state = new SpellState { ShieldTimer = 2f };
        Assert.True(state.HasActiveShield);
    }

    [Fact]
    public void HasActiveShield_FalseWhenTimerZero()
    {
        var state = new SpellState { ShieldTimer = 0f };
        Assert.False(state.HasActiveShield);
    }

    [Fact]
    public void Cooldowns_IndependentPerSpell()
    {
        var state = new SpellState();
        state.Cooldowns[0] = 1.5f;
        state.Cooldowns[2] = 3f;

        Assert.Equal(1.5f, state.Cooldowns[0]);
        Assert.Equal(0f, state.Cooldowns[1]);
        Assert.Equal(3f, state.Cooldowns[2]);
        Assert.Equal(0f, state.Cooldowns[3]);
    }
}
