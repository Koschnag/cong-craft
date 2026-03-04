using System.Numerics;

namespace CongCraft.Engine.VFX;

/// <summary>
/// A single particle with position, velocity, color, lifetime.
/// Struct for cache-friendly iteration.
/// </summary>
public struct Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector4 Color;
    public float Size;
    public float Life;
    public float MaxLife;
    public bool IsAlive => Life > 0;
    public float LifeRatio => MaxLife > 0 ? Life / MaxLife : 0;
}
