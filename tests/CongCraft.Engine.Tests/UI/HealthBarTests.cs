using System.Numerics;
using CongCraft.Engine.UI;

namespace CongCraft.Engine.Tests.UI;

public class HealthBarTests
{
    [Fact]
    public void Calculate_FullHealth_FillEqualsBackground()
    {
        var (bg, fill) = HealthBar.Calculate(100, 100, new Vector2(0, 0), new Vector2(200, 20));
        Assert.Equal(bg.Size.X, fill.Size.X);
    }

    [Fact]
    public void Calculate_HalfHealth_FillIsHalfWidth()
    {
        var (bg, fill) = HealthBar.Calculate(50, 100, new Vector2(0, 0), new Vector2(200, 20));
        Assert.Equal(100, fill.Size.X);
    }

    [Fact]
    public void Calculate_ZeroHealth_FillIsZeroWidth()
    {
        var (bg, fill) = HealthBar.Calculate(0, 100, new Vector2(0, 0), new Vector2(200, 20));
        Assert.Equal(0, fill.Size.X);
    }

    [Fact]
    public void Calculate_SamePosition()
    {
        var pos = new Vector2(50, 30);
        var (bg, fill) = HealthBar.Calculate(75, 100, pos, new Vector2(200, 20));
        Assert.Equal(pos, bg.Position);
        Assert.Equal(pos, fill.Position);
    }
}
