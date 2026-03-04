using System.Numerics;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.VFX;

/// <summary>
/// A point light in the scene (torches, magic effects).
/// </summary>
public sealed class PointLightComponent : IComponent
{
    public Vector3 Color { get; set; } = new(1f, 0.7f, 0.3f);
    public float Intensity { get; set; } = 2f;
    public float Radius { get; set; } = 10f;
    public float FlickerSpeed { get; set; } = 5f;
    public float FlickerAmount { get; set; } = 0.2f;
}

/// <summary>
/// Stores up to 4 active point lights for shader upload.
/// </summary>
public sealed class PointLightData
{
    public const int MaxLights = 4;
    public Vector3[] Positions { get; } = new Vector3[MaxLights];
    public Vector3[] Colors { get; } = new Vector3[MaxLights];
    public float[] Intensities { get; } = new float[MaxLights];
    public float[] Radii { get; } = new float[MaxLights];
    public int Count { get; set; }

    public void ApplyToShader(Rendering.Shader shader)
    {
        shader.SetUniform("uPointLightCount", Count);
        for (int i = 0; i < Count && i < MaxLights; i++)
        {
            shader.SetUniform($"uPointLightPos[{i}]", Positions[i]);
            shader.SetUniform($"uPointLightColor[{i}]", Colors[i]);
            shader.SetUniform($"uPointLightIntensity[{i}]", Intensities[i]);
            shader.SetUniform($"uPointLightRadius[{i}]", Radii[i]);
        }
    }
}
