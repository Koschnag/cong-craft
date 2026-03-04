using System.Numerics;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Configuration for how particles are emitted (burst, continuous, colors, speeds).
/// </summary>
public sealed class ParticleEmitterConfig
{
    public Vector3 Position { get; set; }
    public Vector4 StartColor { get; set; } = new(1, 1, 1, 1);
    public Vector4 EndColor { get; set; } = new(1, 1, 1, 0);
    public float MinSpeed { get; set; } = 1f;
    public float MaxSpeed { get; set; } = 3f;
    public float MinLife { get; set; } = 0.3f;
    public float MaxLife { get; set; } = 1f;
    public float MinSize { get; set; } = 0.05f;
    public float MaxSize { get; set; } = 0.15f;
    public Vector3 Gravity { get; set; } = new(0, -3f, 0);
    public float SpreadAngle { get; set; } = MathF.PI; // Full sphere
    public Vector3 EmitDirection { get; set; } = Vector3.UnitY;
    public int MaxParticles { get; set; } = 50;
}
