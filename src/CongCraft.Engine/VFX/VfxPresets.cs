using System.Numerics;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Predefined particle effect configurations for common game effects.
/// </summary>
public static class VfxPresets
{
    public static ParticleEmitterConfig SwordSwing => new()
    {
        StartColor = new Vector4(1f, 0.8f, 0.3f, 0.9f),
        EndColor = new Vector4(1f, 0.5f, 0.1f, 0f),
        MinSpeed = 2f,
        MaxSpeed = 4f,
        MinLife = 0.1f,
        MaxLife = 0.25f,
        MinSize = 0.02f,
        MaxSize = 0.06f,
        Gravity = Vector3.Zero,
        SpreadAngle = MathF.PI * 0.3f,
        EmitDirection = Vector3.UnitZ,
        MaxParticles = 20
    };

    public static ParticleEmitterConfig HitSparks => new()
    {
        StartColor = new Vector4(1f, 0.9f, 0.5f, 1f),
        EndColor = new Vector4(0.8f, 0.3f, 0.1f, 0f),
        MinSpeed = 3f,
        MaxSpeed = 8f,
        MinLife = 0.15f,
        MaxLife = 0.4f,
        MinSize = 0.02f,
        MaxSize = 0.05f,
        Gravity = new Vector3(0, -6f, 0),
        SpreadAngle = MathF.PI * 0.5f,
        EmitDirection = Vector3.UnitY,
        MaxParticles = 30
    };

    public static ParticleEmitterConfig BloodSplatter => new()
    {
        StartColor = new Vector4(0.6f, 0.05f, 0.05f, 0.9f),
        EndColor = new Vector4(0.3f, 0.02f, 0.02f, 0f),
        MinSpeed = 2f,
        MaxSpeed = 5f,
        MinLife = 0.3f,
        MaxLife = 0.7f,
        MinSize = 0.03f,
        MaxSize = 0.08f,
        Gravity = new Vector3(0, -8f, 0),
        SpreadAngle = MathF.PI * 0.6f,
        EmitDirection = Vector3.UnitY,
        MaxParticles = 25
    };

    public static ParticleEmitterConfig TorchFire => new()
    {
        StartColor = new Vector4(1f, 0.6f, 0.1f, 0.8f),
        EndColor = new Vector4(0.8f, 0.2f, 0.0f, 0f),
        MinSpeed = 0.5f,
        MaxSpeed = 1.5f,
        MinLife = 0.3f,
        MaxLife = 0.8f,
        MinSize = 0.04f,
        MaxSize = 0.12f,
        Gravity = new Vector3(0, 1.5f, 0), // Fire rises
        SpreadAngle = MathF.PI * 0.2f,
        EmitDirection = Vector3.UnitY,
        MaxParticles = 40
    };

    public static ParticleEmitterConfig TorchEmbers => new()
    {
        StartColor = new Vector4(1f, 0.7f, 0.2f, 0.7f),
        EndColor = new Vector4(0.5f, 0.2f, 0.0f, 0f),
        MinSpeed = 0.3f,
        MaxSpeed = 1f,
        MinLife = 0.5f,
        MaxLife = 1.5f,
        MinSize = 0.01f,
        MaxSize = 0.03f,
        Gravity = new Vector3(0, 0.5f, 0),
        SpreadAngle = MathF.PI * 0.4f,
        EmitDirection = Vector3.UnitY,
        MaxParticles = 15
    };
}
