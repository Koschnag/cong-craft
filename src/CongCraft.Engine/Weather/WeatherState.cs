using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Weather;

/// <summary>
/// Singleton tracking current weather conditions and transitions.
/// </summary>
public sealed class WeatherState : IComponent
{
    public WeatherType Current { get; set; } = WeatherType.Clear;
    public WeatherType Target { get; set; } = WeatherType.Clear;
    public float Intensity { get; set; } // 0..1 blend toward target weather
    public float TimeUntilChange { get; set; } = 60f; // Seconds until next weather change
    public float TransitionSpeed { get; set; } = 0.3f; // How fast to blend (per second)

    /// <summary>
    /// Effective intensity of the current weather effect (rain, fog, etc.).
    /// </summary>
    public float EffectiveIntensity => Current == Target ? Intensity : Intensity;
}

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    HeavyRain,
    Fog,
    Storm
}
