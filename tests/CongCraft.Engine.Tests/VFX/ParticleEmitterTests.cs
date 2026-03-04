using System.Numerics;
using CongCraft.Engine.VFX;

namespace CongCraft.Engine.Tests.VFX;

public class ParticleEmitterTests
{
    [Fact]
    public void NewEmitter_NoAliveParticles()
    {
        var config = new ParticleEmitterConfig { MaxParticles = 10 };
        var emitter = new ParticleEmitter(config);
        Assert.Equal(0, emitter.AliveCount);
    }

    [Fact]
    public void Emit_CreatesParticles()
    {
        var config = new ParticleEmitterConfig
        {
            MaxParticles = 20, MinLife = 1f, MaxLife = 2f,
            MinSpeed = 1f, MaxSpeed = 2f
        };
        var emitter = new ParticleEmitter(config, seed: 42);
        emitter.Emit(5);
        emitter.Update(0.01f); // One small tick to count alive
        Assert.Equal(5, emitter.AliveCount);
    }

    [Fact]
    public void Update_ParticlesDieOverTime()
    {
        var config = new ParticleEmitterConfig
        {
            MaxParticles = 10, MinLife = 0.1f, MaxLife = 0.1f,
            MinSpeed = 1f, MaxSpeed = 1f, Gravity = Vector3.Zero
        };
        var emitter = new ParticleEmitter(config, seed: 42);
        emitter.Emit(5);
        emitter.Update(0.05f);
        Assert.Equal(5, emitter.AliveCount);
        emitter.Update(0.06f); // Total > 0.1, all die
        Assert.Equal(0, emitter.AliveCount);
    }

    [Fact]
    public void Update_ParticlesMove()
    {
        var config = new ParticleEmitterConfig
        {
            MaxParticles = 5, MinLife = 1f, MaxLife = 1f,
            MinSpeed = 5f, MaxSpeed = 5f,
            Gravity = Vector3.Zero,
            Position = Vector3.Zero
        };
        var emitter = new ParticleEmitter(config, seed: 42);
        emitter.Emit(1);
        emitter.Update(0.5f);

        // Particle should have moved from origin
        bool moved = false;
        foreach (ref readonly var p in emitter.Particles)
        {
            if (p.IsAlive && p.Position != Vector3.Zero)
                moved = true;
        }
        Assert.True(moved, "Particle should move from origin");
    }

    [Fact]
    public void Emit_RespectsMaxParticles()
    {
        var config = new ParticleEmitterConfig { MaxParticles = 3, MinLife = 1f, MaxLife = 1f };
        var emitter = new ParticleEmitter(config, seed: 42);
        emitter.Emit(10); // Emit more than max
        emitter.Update(0.01f);
        Assert.True(emitter.AliveCount <= 3, "Should not exceed max particles");
    }

    [Fact]
    public void Update_GravityAffectsVelocity()
    {
        var config = new ParticleEmitterConfig
        {
            MaxParticles = 5, MinLife = 2f, MaxLife = 2f,
            MinSpeed = 1f, MaxSpeed = 1f,
            Gravity = new Vector3(0, -20f, 0),
            SpreadAngle = 0.01f, // Mostly upward
            EmitDirection = Vector3.UnitY,
            Position = new Vector3(0, 10, 0)
        };
        var emitter = new ParticleEmitter(config, seed: 42);
        emitter.Emit(1);

        // After enough time, gravity should dominate and pull Y down
        emitter.Update(0.1f); // Small tick, particle moves up
        emitter.Update(0.5f); // Larger tick, gravity dominates

        bool foundFalling = false;
        foreach (ref readonly var p in emitter.Particles)
        {
            if (p.IsAlive && p.Velocity.Y < 0)
            {
                foundFalling = true;
                break;
            }
        }
        Assert.True(foundFalling, "Gravity should eventually cause negative Y velocity");
    }
}
