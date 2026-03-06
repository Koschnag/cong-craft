using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Environment;

/// <summary>
/// Advances time of day and updates lighting/sky colors accordingly.
/// One full cycle = 24 in-game hours.
/// </summary>
public sealed class DayNightCycle : ISystem
{
    public int Priority => 30;

    /// <summary>Hours per real-time minute. Default 4 = 6 minutes per full day.</summary>
    public float CycleSpeed { get; set; } = 4f;

    public float TimeOfDay { get; set; } = 10.0f; // Start at mid-morning — textures clearly visible

    private LightingData _lighting = null!;

    // Sky color presets
    private static readonly Vector3 NightZenith = new(0.01f, 0.01f, 0.05f);
    private static readonly Vector3 NightHorizon = new(0.03f, 0.03f, 0.08f);
    private static readonly Vector3 DawnZenith = new(0.18f, 0.10f, 0.28f);   // Deep violet dawn
    private static readonly Vector3 DawnHorizon = new(0.70f, 0.32f, 0.12f);  // Vivid blood-orange
    private static readonly Vector3 DayZenith = new(0.18f, 0.30f, 0.58f);    // Slightly muted blue sky
    private static readonly Vector3 DayHorizon = new(0.48f, 0.52f, 0.62f);   // Slightly muted horizon
    private static readonly Vector3 DuskZenith = new(0.10f, 0.05f, 0.22f);   // Deep purple dusk
    private static readonly Vector3 DuskHorizon = new(0.68f, 0.22f, 0.06f);  // Blood-orange sunset

    public Vector3 ZenithColor { get; private set; } = DayZenith;
    public Vector3 HorizonColor { get; private set; } = DayHorizon;

    public Vector3 SunDirection
    {
        get
        {
            float angle = (TimeOfDay / 24f) * MathF.Tau - MathF.PI / 2f;
            return Vector3.Normalize(new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0.3f));
        }
    }

    public void Initialize(ServiceLocator services)
    {
        _lighting = services.Get<LightingData>();
        UpdateLighting();
    }

    public void Update(GameTime time)
    {
        TimeOfDay = (TimeOfDay + (float)time.DeltaTime * CycleSpeed / 60f) % 24f;
        UpdateLighting();
    }

    private void UpdateLighting()
    {
        float t = TimeOfDay;

        // Sun direction
        _lighting.SunDirection = SunDirection;

        // Sun intensity and color based on time
        if (t < 5f) // Night
        {
            _lighting.SunIntensity = 0.05f;
            _lighting.SunColor = new Vector3(0.2f, 0.2f, 0.4f);
            _lighting.AmbientColor = new Vector3(0.03f, 0.03f, 0.06f);
            ZenithColor = NightZenith;
            HorizonColor = NightHorizon;
            _lighting.FogColor = new Vector3(0.02f, 0.02f, 0.04f);
        }
        else if (t < 7f) // Dawn
        {
            float f = (t - 5f) / 2f;
            _lighting.SunIntensity = Lerp(0.05f, 0.7f, f);
            _lighting.SunColor = Vector3.Lerp(new Vector3(0.2f, 0.2f, 0.4f), new Vector3(1f, 0.6f, 0.3f), f);
            _lighting.AmbientColor = Vector3.Lerp(new Vector3(0.03f, 0.03f, 0.06f), new Vector3(0.15f, 0.1f, 0.08f), f);
            ZenithColor = Vector3.Lerp(NightZenith, DawnZenith, f);
            HorizonColor = Vector3.Lerp(NightHorizon, DawnHorizon, f);
            _lighting.FogColor = Vector3.Lerp(new Vector3(0.02f, 0.02f, 0.04f), new Vector3(0.5f, 0.35f, 0.25f), f);
        }
        else if (t < 9f) // Morning
        {
            float f = (t - 7f) / 2f;
            _lighting.SunIntensity = Lerp(0.7f, 1f, f);
            _lighting.SunColor = Vector3.Lerp(new Vector3(1f, 0.6f, 0.3f), new Vector3(1f, 0.95f, 0.8f), f);
            _lighting.AmbientColor = Vector3.Lerp(new Vector3(0.15f, 0.1f, 0.08f), new Vector3(0.15f, 0.15f, 0.2f), f);
            ZenithColor = Vector3.Lerp(DawnZenith, DayZenith, f);
            HorizonColor = Vector3.Lerp(DawnHorizon, DayHorizon, f);
            _lighting.FogColor = Vector3.Lerp(new Vector3(0.5f, 0.35f, 0.25f), new Vector3(0.5f, 0.5f, 0.55f), f);
        }
        else if (t < 17f) // Day
        {
            _lighting.SunIntensity = 0.92f; // Slightly muted — not overly bright
            _lighting.SunColor = new Vector3(1f, 0.93f, 0.78f);
            _lighting.AmbientColor = new Vector3(0.12f, 0.13f, 0.18f);
            ZenithColor = DayZenith;
            HorizonColor = DayHorizon;
            _lighting.FogColor = new Vector3(0.5f, 0.5f, 0.55f);
        }
        else if (t < 19f) // Dusk
        {
            float f = (t - 17f) / 2f;
            _lighting.SunIntensity = Lerp(0.95f, 0.25f, f);  // Faster falloff
            _lighting.SunColor = Vector3.Lerp(new Vector3(1f, 0.80f, 0.55f), new Vector3(0.9f, 0.30f, 0.08f), f); // Deeper red
            _lighting.AmbientColor = Vector3.Lerp(new Vector3(0.12f, 0.12f, 0.18f), new Vector3(0.07f, 0.04f, 0.09f), f);
            ZenithColor = Vector3.Lerp(DayZenith, DuskZenith, f);
            HorizonColor = Vector3.Lerp(DayHorizon, DuskHorizon, f);
            _lighting.FogColor = Vector3.Lerp(new Vector3(0.45f, 0.42f, 0.48f), new Vector3(0.32f, 0.12f, 0.08f), f);
        }
        else // Night transition
        {
            float f = (t - 19f) / 2f;
            f = Math.Clamp(f, 0f, 1f);
            _lighting.SunIntensity = Lerp(0.4f, 0.05f, f);
            _lighting.SunColor = Vector3.Lerp(new Vector3(1f, 0.4f, 0.15f), new Vector3(0.2f, 0.2f, 0.4f), f);
            _lighting.AmbientColor = Vector3.Lerp(new Vector3(0.1f, 0.05f, 0.08f), new Vector3(0.03f, 0.03f, 0.06f), f);
            ZenithColor = Vector3.Lerp(DuskZenith, NightZenith, f);
            HorizonColor = Vector3.Lerp(DuskHorizon, NightHorizon, f);
            _lighting.FogColor = Vector3.Lerp(new Vector3(0.3f, 0.15f, 0.1f), new Vector3(0.02f, 0.02f, 0.04f), f);
        }
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    public void Render(GameTime time) { }
    public void Dispose() { }
}
