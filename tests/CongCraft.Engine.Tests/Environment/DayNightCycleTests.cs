using CongCraft.Engine.Core;
using CongCraft.Engine.Environment;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Tests.Environment;

public class DayNightCycleTests
{
    private DayNightCycle CreateCycle(float timeOfDay)
    {
        var cycle = new DayNightCycle { TimeOfDay = timeOfDay };
        var services = new ServiceLocator();
        services.Register(new LightingData());
        cycle.Initialize(services);
        return cycle;
    }

    [Theory]
    [InlineData(12f)]  // Noon
    [InlineData(10f)]  // Morning
    [InlineData(14f)]  // Afternoon
    public void SunDirection_DuringDay_SunIsUp(float hour)
    {
        var cycle = CreateCycle(hour);
        Assert.True(cycle.SunDirection.Y > 0, $"Sun should be up at hour {hour}");
    }

    [Theory]
    [InlineData(0f)]   // Midnight
    [InlineData(2f)]   // Deep night
    [InlineData(23f)]  // Late night
    public void SunDirection_DuringNight_SunIsDown(float hour)
    {
        var cycle = CreateCycle(hour);
        Assert.True(cycle.SunDirection.Y < 0, $"Sun should be down at hour {hour}");
    }

    [Fact]
    public void TimeOfDay_Wraps_At_24()
    {
        var cycle = CreateCycle(23.5f);
        // Advance time enough to wrap
        cycle.Update(new GameTime(60 * 60, 60 * 60, 1)); // Huge delta
        Assert.InRange(cycle.TimeOfDay, 0f, 24f);
    }

    [Fact]
    public void TimeOfDay_Advances()
    {
        var cycle = CreateCycle(10f);
        float before = cycle.TimeOfDay;
        cycle.Update(new GameTime(1.0, 1.0, 1)); // 1 second
        Assert.True(cycle.TimeOfDay > before);
    }

    [Fact]
    public void ZenithColor_BrighterDuringDay()
    {
        var day = CreateCycle(12f);
        var night = CreateCycle(0f);

        float dayBrightness = day.ZenithColor.X + day.ZenithColor.Y + day.ZenithColor.Z;
        float nightBrightness = night.ZenithColor.X + night.ZenithColor.Y + night.ZenithColor.Z;

        Assert.True(dayBrightness > nightBrightness);
    }
}
