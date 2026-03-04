using System.Numerics;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Manages a pool of particles, emitting and updating them.
/// </summary>
public sealed class ParticleEmitter
{
    private readonly Particle[] _particles;
    private readonly ParticleEmitterConfig _config;
    private readonly Random _rng;
    private int _nextParticle;

    public ParticleEmitter(ParticleEmitterConfig config, int seed = 0)
    {
        _config = config;
        _particles = new Particle[config.MaxParticles];
        _rng = new Random(seed);
    }

    public ParticleEmitterConfig Config => _config;
    public ReadOnlySpan<Particle> Particles => _particles;
    public int AliveCount { get; private set; }

    public void Emit(int count, Vector3? position = null)
    {
        var emitPos = position ?? _config.Position;
        for (int i = 0; i < count; i++)
        {
            ref var p = ref _particles[_nextParticle % _particles.Length];
            _nextParticle++;

            p.Position = emitPos;
            p.Velocity = RandomDirection() * Lerp(_config.MinSpeed, _config.MaxSpeed, (float)_rng.NextDouble());
            p.Color = _config.StartColor;
            p.Size = Lerp(_config.MinSize, _config.MaxSize, (float)_rng.NextDouble());
            p.MaxLife = Lerp(_config.MinLife, _config.MaxLife, (float)_rng.NextDouble());
            p.Life = p.MaxLife;
        }
    }

    public void Update(float dt)
    {
        int alive = 0;
        for (int i = 0; i < _particles.Length; i++)
        {
            ref var p = ref _particles[i];
            if (!p.IsAlive) continue;

            p.Life -= dt;
            if (!p.IsAlive) continue;

            p.Position += p.Velocity * dt;
            p.Velocity += _config.Gravity * dt;

            // Interpolate color based on lifetime
            float t = 1f - p.LifeRatio;
            p.Color = Vector4.Lerp(_config.StartColor, _config.EndColor, t);
            p.Size = Lerp(_config.MaxSize, _config.MinSize * 0.5f, t);
            alive++;
        }
        AliveCount = alive;
    }

    private Vector3 RandomDirection()
    {
        float theta = (float)(_rng.NextDouble() * MathF.Tau);
        float phi = (float)(_rng.NextDouble() * _config.SpreadAngle);

        float sinPhi = MathF.Sin(phi);
        var baseDir = new Vector3(
            sinPhi * MathF.Cos(theta),
            MathF.Cos(phi),
            sinPhi * MathF.Sin(theta));

        // Rotate to align with emit direction
        if (Vector3.Dot(_config.EmitDirection, Vector3.UnitY) < 0.999f)
        {
            var axis = Vector3.Cross(Vector3.UnitY, _config.EmitDirection);
            if (axis.LengthSquared() > 0.0001f)
            {
                axis = Vector3.Normalize(axis);
                float angle = MathF.Acos(MathF.Min(1f, Vector3.Dot(Vector3.UnitY, _config.EmitDirection)));
                var rot = Quaternion.CreateFromAxisAngle(axis, angle);
                baseDir = Vector3.Transform(baseDir, rot);
            }
        }

        return baseDir;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
