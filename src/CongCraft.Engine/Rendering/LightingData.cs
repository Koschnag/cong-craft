using System.Numerics;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Global lighting state passed to all shaders.
/// </summary>
public sealed class LightingData
{
    public Vector3 SunDirection { get; set; } = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
    public Vector3 SunColor { get; set; } = new(1.0f, 0.95f, 0.8f);
    public float SunIntensity { get; set; } = 1.0f;
    public Vector3 AmbientColor { get; set; } = new(0.15f, 0.15f, 0.2f);
    public Vector3 FogColor { get; set; } = new(0.5f, 0.5f, 0.55f);
    public float FogDensity { get; set; } = 0.005f;

    public void ApplyToShader(Shader shader)
    {
        shader.SetUniform("uSunDirection", SunDirection);
        shader.SetUniform("uSunColor", SunColor);
        shader.SetUniform("uSunIntensity", SunIntensity);
        shader.SetUniform("uAmbientColor", AmbientColor);
        shader.SetUniform("uFogColor", FogColor);
        shader.SetUniform("uFogDensity", FogDensity);
    }
}
