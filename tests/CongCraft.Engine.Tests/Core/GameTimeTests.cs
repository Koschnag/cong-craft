using CongCraft.Engine.Core;

namespace CongCraft.Engine.Tests.Core;

public class GameTimeTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var gt = new GameTime(0.016, 1.5, 90);
        Assert.Equal(0.016, gt.DeltaTime);
        Assert.Equal(1.5, gt.TotalTime);
        Assert.Equal(90, gt.FrameCount);
    }

    [Fact]
    public void DeltaTimeF_ReturnsSinglePrecision()
    {
        var gt = new GameTime(0.016, 1.5, 1);
        Assert.Equal(0.016f, gt.DeltaTimeF, 0.0001f);
    }

    [Fact]
    public void TotalTimeF_ReturnsSinglePrecision()
    {
        var gt = new GameTime(0.016, 10.5, 1);
        Assert.Equal(10.5f, gt.TotalTimeF, 0.01f);
    }

    [Fact]
    public void Default_HasZeroValues()
    {
        var gt = default(GameTime);
        Assert.Equal(0, gt.DeltaTime);
        Assert.Equal(0, gt.TotalTime);
        Assert.Equal(0, gt.FrameCount);
    }
}
