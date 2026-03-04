using System.Numerics;

namespace CongCraft.Engine.Weather;

/// <summary>
/// Defines visual parameters for each weather type.
/// </summary>
public static class WeatherConfig
{
    public static WeatherParams GetParams(WeatherType type) => type switch
    {
        WeatherType.Clear => new WeatherParams
        {
            FogDensity = 0.005f,
            FogColor = new Vector3(0.5f, 0.5f, 0.55f),
            AmbientMod = new Vector3(0f, 0f, 0f),
            SunIntensityMod = 0f,
            RainIntensity = 0f,
            LightningChance = 0f
        },
        WeatherType.Cloudy => new WeatherParams
        {
            FogDensity = 0.008f,
            FogColor = new Vector3(0.45f, 0.45f, 0.5f),
            AmbientMod = new Vector3(-0.03f, -0.03f, -0.02f),
            SunIntensityMod = -0.3f,
            RainIntensity = 0f,
            LightningChance = 0f
        },
        WeatherType.Rain => new WeatherParams
        {
            FogDensity = 0.012f,
            FogColor = new Vector3(0.4f, 0.42f, 0.48f),
            AmbientMod = new Vector3(-0.05f, -0.05f, -0.03f),
            SunIntensityMod = -0.5f,
            RainIntensity = 0.6f,
            LightningChance = 0f
        },
        WeatherType.HeavyRain => new WeatherParams
        {
            FogDensity = 0.02f,
            FogColor = new Vector3(0.35f, 0.38f, 0.45f),
            AmbientMod = new Vector3(-0.08f, -0.08f, -0.05f),
            SunIntensityMod = -0.7f,
            RainIntensity = 1f,
            LightningChance = 0f
        },
        WeatherType.Fog => new WeatherParams
        {
            FogDensity = 0.04f,
            FogColor = new Vector3(0.55f, 0.55f, 0.58f),
            AmbientMod = new Vector3(-0.02f, -0.02f, -0.01f),
            SunIntensityMod = -0.4f,
            RainIntensity = 0f,
            LightningChance = 0f
        },
        WeatherType.Storm => new WeatherParams
        {
            FogDensity = 0.025f,
            FogColor = new Vector3(0.3f, 0.32f, 0.4f),
            AmbientMod = new Vector3(-0.1f, -0.1f, -0.07f),
            SunIntensityMod = -0.8f,
            RainIntensity = 1f,
            LightningChance = 0.02f // 2% chance per second
        },
        _ => GetParams(WeatherType.Clear)
    };
}

public sealed class WeatherParams
{
    public float FogDensity { get; init; }
    public Vector3 FogColor { get; init; }
    public Vector3 AmbientMod { get; init; }
    public float SunIntensityMod { get; init; }
    public float RainIntensity { get; init; }
    public float LightningChance { get; init; }
}
