using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.VFX;

namespace CongCraft.Engine.Weather;

/// <summary>
/// Manages weather transitions, applies visual effects to lighting,
/// and controls rain particles. Weather changes randomly over time.
/// </summary>
public sealed class WeatherSystem : ISystem
{
    public int Priority => 3; // Very early, before rendering

    private World _world = null!;
    private LightingData _lighting = null!;
    private Camera _camera = null!;
    private Random _rng = new(42);

    // Base lighting values (from DayNightCycle, saved before weather modifies them)
    private Vector3 _baseFogColor;
    private float _baseFogDensity;
    private float _baseSunIntensity;
    private Vector3 _baseAmbient;

    // Rain particle emitter
    private ParticleEmitter _rainEmitter = null!;

    // Lightning flash state
    public float LightningFlash { get; private set; }

    // Weather transition weights (what weather can follow what)
    private static readonly WeatherType[] WeatherOptions =
    {
        WeatherType.Clear, WeatherType.Clear, WeatherType.Clear,  // Clear is most common
        WeatherType.Cloudy, WeatherType.Cloudy,
        WeatherType.Rain,
        WeatherType.HeavyRain,
        WeatherType.Fog,
        WeatherType.Storm
    };

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _lighting = services.Get<LightingData>();
        _camera = services.Get<Camera>();

        _world.SetSingleton(new WeatherState());

        // Rain emitter config
        var rainConfig = new ParticleEmitterConfig
        {
            MaxParticles = 200,
            MinLife = 0.8f,
            MaxLife = 1.5f,
            MinSpeed = 8f,
            MaxSpeed = 12f,
            MinSize = 0.02f,
            MaxSize = 0.04f,
            Gravity = new Vector3(0, -15f, 0),
            EmitDirection = new Vector3(0, -1, 0),
            SpreadAngle = 0.3f,
            StartColor = new Vector4(0.6f, 0.65f, 0.8f, 0.6f),
            EndColor = new Vector4(0.5f, 0.55f, 0.7f, 0.1f)
        };
        _rainEmitter = new ParticleEmitter(rainConfig, 555);
        ParticleRenderSystem.RegisterEmitter(_rainEmitter);
    }

    public void Update(GameTime time)
    {
        float dt = time.DeltaTimeF;
        var weather = _world.GetSingleton<WeatherState>();

        // Save base lighting values on first frame
        if (time.FrameCount <= 1)
        {
            _baseFogColor = _lighting.FogColor;
            _baseFogDensity = _lighting.FogDensity;
            _baseSunIntensity = _lighting.SunIntensity;
            _baseAmbient = _lighting.AmbientColor;
        }
        else
        {
            // Capture base values before weather mods (DayNightCycle updates these)
            // We approximate by storing the un-weathered values
            _baseFogDensity = 0.005f; // Default clear weather
            _baseSunIntensity = _lighting.SunIntensity - WeatherConfig.GetParams(weather.Current).SunIntensityMod * weather.Intensity;
        }

        // Weather change timer
        weather.TimeUntilChange -= dt;
        if (weather.TimeUntilChange <= 0)
        {
            ChangeWeather(weather);
        }

        // Transition intensity
        if (weather.Current == weather.Target)
        {
            weather.Intensity = MathF.Min(1f, weather.Intensity + weather.TransitionSpeed * dt);
        }
        else
        {
            // Fade out current weather before switching
            weather.Intensity -= weather.TransitionSpeed * dt;
            if (weather.Intensity <= 0)
            {
                weather.Current = weather.Target;
                weather.Intensity = 0;
            }
        }

        // Apply weather effects to lighting
        ApplyWeatherToLighting(weather);

        // Rain particles
        UpdateRain(weather, dt);

        // Lightning
        UpdateLightning(weather, dt);
    }

    private void ChangeWeather(WeatherState weather)
    {
        var newWeather = WeatherOptions[_rng.Next(WeatherOptions.Length)];
        weather.Target = newWeather;
        weather.TimeUntilChange = 30f + (float)_rng.NextDouble() * 90f; // 30-120 seconds
    }

    private void ApplyWeatherToLighting(WeatherState weather)
    {
        var currentParams = WeatherConfig.GetParams(weather.Current);
        float t = weather.Intensity;

        // Modify fog
        _lighting.FogDensity = MathF.Max(0.001f, _baseFogDensity + currentParams.FogDensity * t);
        _lighting.FogColor = Vector3.Lerp(_baseFogColor, currentParams.FogColor, t);

        // Modify sun intensity
        float sunMod = currentParams.SunIntensityMod * t;
        _lighting.SunIntensity = MathF.Max(0.05f, _baseSunIntensity + sunMod);

        // Modify ambient
        _lighting.AmbientColor = _baseAmbient + currentParams.AmbientMod * t;

        // Lightning flash
        if (LightningFlash > 0)
        {
            _lighting.SunIntensity += LightningFlash * 3f;
            _lighting.AmbientColor += new Vector3(LightningFlash * 0.5f);
        }
    }

    private void UpdateRain(WeatherState weather, float dt)
    {
        var currentParams = WeatherConfig.GetParams(weather.Current);
        float rainIntensity = currentParams.RainIntensity * weather.Intensity;

        if (rainIntensity > 0.01f)
        {
            // Spawn rain around camera
            var camTarget = _camera.Target;
            int rainCount = (int)(rainIntensity * 5f); // Up to 5 particles per frame

            for (int i = 0; i < rainCount; i++)
            {
                float offsetX = ((float)_rng.NextDouble() - 0.5f) * 20f;
                float offsetZ = ((float)_rng.NextDouble() - 0.5f) * 20f;
                var rainPos = new Vector3(camTarget.X + offsetX, camTarget.Y + 15f, camTarget.Z + offsetZ);
                _rainEmitter.Emit(1, rainPos);
            }
        }
    }

    private void UpdateLightning(WeatherState weather, float dt)
    {
        LightningFlash = MathF.Max(0, LightningFlash - dt * 4f);

        var currentParams = WeatherConfig.GetParams(weather.Current);
        if (currentParams.LightningChance > 0 && weather.Intensity > 0.5f)
        {
            if (_rng.NextDouble() < currentParams.LightningChance * dt)
            {
                LightningFlash = 1f;
            }
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
