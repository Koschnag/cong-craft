using CongCraft.Engine.Weather;

namespace CongCraft.Engine.Tests.Weather;

public class WeatherStateTests
{
    [Fact]
    public void DefaultValues()
    {
        var state = new WeatherState();
        Assert.Equal(WeatherType.Clear, state.Current);
        Assert.Equal(WeatherType.Clear, state.Target);
        Assert.Equal(0f, state.Intensity);
        Assert.True(state.TimeUntilChange > 0);
    }

    [Fact]
    public void EffectiveIntensity_MatchesIntensity()
    {
        var state = new WeatherState { Intensity = 0.7f };
        Assert.Equal(0.7f, state.EffectiveIntensity);
    }

    [Theory]
    [InlineData(WeatherType.Clear)]
    [InlineData(WeatherType.Cloudy)]
    [InlineData(WeatherType.Rain)]
    [InlineData(WeatherType.HeavyRain)]
    [InlineData(WeatherType.Fog)]
    [InlineData(WeatherType.Storm)]
    public void AllWeatherTypes_HaveConfigs(WeatherType type)
    {
        var config = WeatherConfig.GetParams(type);
        Assert.NotNull(config);
        Assert.True(config.FogDensity > 0);
    }
}
