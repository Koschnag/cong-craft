using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Triggers particle effects during combat: sword swings, hit sparks, blood on damage.
/// Monitors combat state changes and health changes to trigger appropriate effects.
/// </summary>
public sealed class CombatVfxSystem : ISystem
{
    public int Priority => 16; // Right after combat system

    private World _world = null!;
    private Camera _camera = null!;

    private ParticleEmitter _swingEmitter = null!;
    private ParticleEmitter _sparkEmitter = null!;
    private ParticleEmitter _bloodEmitter = null!;

    // Track previous state to detect changes
    private bool _wasAttacking;
    private readonly Dictionary<int, float> _previousHealth = new();

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _camera = services.Get<Camera>();

        _swingEmitter = new ParticleEmitter(VfxPresets.SwordSwing, 111);
        _sparkEmitter = new ParticleEmitter(VfxPresets.HitSparks, 222);
        _bloodEmitter = new ParticleEmitter(VfxPresets.BloodSplatter, 333);

        ParticleRenderSystem.RegisterEmitter(_swingEmitter);
        ParticleRenderSystem.RegisterEmitter(_sparkEmitter);
        ParticleRenderSystem.RegisterEmitter(_bloodEmitter);
    }

    public void Update(GameTime time)
    {
        // Player attack swing effect
        foreach (var (entity, player, combat, transform) in QueryPlayerCombat())
        {
            if (combat.IsAttacking && !_wasAttacking)
            {
                // Sword swing: emit at player position + forward
                var emitPos = transform.Position + _camera.Forward * 1f + Vector3.UnitY * 0.8f;
                _swingEmitter.Config.EmitDirection = _camera.Forward;
                _swingEmitter.Emit(8, emitPos);
            }
            _wasAttacking = combat.IsAttacking;
        }

        // Enemy damage effects (sparks + blood)
        foreach (var (entity, enemy, health, transform) in QueryEnemyHealth())
        {
            float prevHealth = _previousHealth.GetValueOrDefault(entity.Id, health.Max);

            if (health.Current < prevHealth && health.Current >= 0)
            {
                // Hit! Sparks + blood at enemy position
                var hitPos = transform.Position + Vector3.UnitY * 0.7f;
                _sparkEmitter.Emit(12, hitPos);
                _bloodEmitter.Emit(8, hitPos);
            }

            _previousHealth[entity.Id] = health.Current;
        }

        // Player damage effect (blood only, less)
        foreach (var (entity, player, health, transform) in QueryPlayerHealth())
        {
            float prevHealth = _previousHealth.GetValueOrDefault(entity.Id + 10000, health.Max);

            if (health.Current < prevHealth)
            {
                var hitPos = transform.Position + Vector3.UnitY * 0.8f;
                _bloodEmitter.Emit(5, hitPos);
            }

            _previousHealth[entity.Id + 10000] = health.Current;
        }
    }

    private IEnumerable<(Entity, PlayerComponent, CombatComponent, TransformComponent)> QueryPlayerCombat()
    {
        foreach (var (entity, player, combat) in _world.Query<PlayerComponent, CombatComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, player, combat, _world.GetComponent<TransformComponent>(entity));
        }
    }

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryEnemyHealth()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    private IEnumerable<(Entity, PlayerComponent, HealthComponent, TransformComponent)> QueryPlayerHealth()
    {
        foreach (var (entity, player, health) in _world.Query<PlayerComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, player, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        ParticleRenderSystem.UnregisterEmitter(_swingEmitter);
        ParticleRenderSystem.UnregisterEmitter(_sparkEmitter);
        ParticleRenderSystem.UnregisterEmitter(_bloodEmitter);
    }
}
