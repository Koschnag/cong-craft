using CongCraft.Engine.Weather;

namespace CongCraft.Engine.Tests.Weather;

public class WeatherConfigTests
{
    [Fact]
    public void Clear_HasLowFog()
    {
        var config = WeatherConfig.GetParams(WeatherType.Clear);
        Assert.Equal(0.005f, config.FogDensity);
        Assert.Equal(0f, config.RainIntensity);
        Assert.Equal(0f, config.SunIntensityMod);
    }

    [Fact]
    public void Rain_HasRainAndReducedSun()
    {
        var config = WeatherConfig.GetParams(WeatherType.Rain);
        Assert.True(config.RainIntensity > 0);
        Assert.True(config.SunIntensityMod < 0);
        Assert.True(config.FogDensity > 0.005f);
    }

    [Fact]
    public void Storm_HasHighestIntensity()
    {
        var storm = WeatherConfig.GetParams(WeatherType.Storm);
        var clear = WeatherConfig.GetParams(WeatherType.Clear);
        Assert.True(storm.SunIntensityMod < clear.SunIntensityMod);
        Assert.True(storm.RainIntensity > clear.RainIntensity);
        Assert.True(storm.LightningChance > 0);
    }

    [Fact]
    public void Fog_HasHighFogDensity()
    {
        var fog = WeatherConfig.GetParams(WeatherType.Fog);
        var clear = WeatherConfig.GetParams(WeatherType.Clear);
        Assert.True(fog.FogDensity > clear.FogDensity * 4);
    }

    [Fact]
    public void HeavyRain_StrongerThanRain()
    {
        var rain = WeatherConfig.GetParams(WeatherType.Rain);
        var heavy = WeatherConfig.GetParams(WeatherType.HeavyRain);
        Assert.True(heavy.RainIntensity > rain.RainIntensity);
        Assert.True(heavy.SunIntensityMod < rain.SunIntensityMod);
    }

    [Fact]
    public void Cloudy_NoRainButReducedSun()
    {
        var config = WeatherConfig.GetParams(WeatherType.Cloudy);
        Assert.Equal(0f, config.RainIntensity);
        Assert.True(config.SunIntensityMod < 0);
    }

    [Fact]
    public void Storm_HasLightningChance()
    {
        var storm = WeatherConfig.GetParams(WeatherType.Storm);
        Assert.True(storm.LightningChance > 0);

        // Other weather types should NOT have lightning
        Assert.Equal(0f, WeatherConfig.GetParams(WeatherType.Clear).LightningChance);
        Assert.Equal(0f, WeatherConfig.GetParams(WeatherType.Rain).LightningChance);
    }
}
