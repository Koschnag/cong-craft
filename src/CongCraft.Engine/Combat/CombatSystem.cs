using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using Silk.NET.Input;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Handles player combat actions: attack (left click), block (right click), dodge (space).
/// Checks hit detection against enemies in range.
/// </summary>
public sealed class CombatSystem : ISystem
{
    public int Priority => 15; // After input, before AI

    private World _world = null!;
    private Camera _camera = null!;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;

        foreach (var (entity, player, combat, health) in QueryPlayerCombat())
        {
            var transform = _world.GetComponent<TransformComponent>(entity);
            UpdateTimers(combat, dt);

            // Attack: left mouse button
            if (input.IsKeyDown(Key.Q) && combat.AttackTimer <= 0 && !combat.IsBlocking && !combat.IsDodging)
            {
                StartAttack(combat);
            }

            // Block: right mouse button / E key
            combat.IsBlocking = input.IsKeyDown(Key.E) && !combat.IsAttacking && !combat.IsDodging;
            if (combat.IsBlocking)
            {
                // Slow down while blocking (handled in movement system via combat state)
            }

            // Dodge: Space
            if (input.IsKeyPressed(Key.Space) && combat.DodgeCooldownTimer <= 0 && !combat.IsAttacking)
            {
                StartDodge(combat, transform, input);
            }

            // Process attack hit at mid-animation
            if (combat.IsAttacking && combat.AttackAnimationProgress > 0.3f && combat.AttackAnimationProgress < 0.5f)
            {
                ProcessPlayerAttackHits(transform, combat);
            }

            // Update attack animation
            if (combat.IsAttacking)
            {
                combat.AttackAnimationProgress += dt / combat.EffectiveAttackCooldown;
                if (combat.AttackAnimationProgress >= 1f)
                {
                    combat.IsAttacking = false;
                    combat.AttackAnimationProgress = 0;
                }
            }

            // Update dodge movement
            if (combat.IsDodging)
            {
                // Dodge movement is applied in PlayerMovementSystem based on IsDodging
                combat.DodgeTimer -= dt;
                if (combat.DodgeTimer <= 0)
                {
                    combat.IsDodging = false;
                }
            }

            // Update health flash
            health.DamageFlashTimer = MathF.Max(0, health.DamageFlashTimer - dt);
        }
    }

    private void StartAttack(CombatComponent combat)
    {
        combat.IsAttacking = true;
        combat.AttackTimer = combat.EffectiveAttackCooldown;
        combat.AttackAnimationProgress = 0;
    }

    private void StartDodge(CombatComponent combat, TransformComponent transform, InputState input)
    {
        combat.IsDodging = true;
        combat.DodgeTimer = combat.DodgeDuration;
        combat.DodgeCooldownTimer = combat.DodgeCooldown;
    }

    private void ProcessPlayerAttackHits(TransformComponent playerTransform, CombatComponent playerCombat)
    {
        var playerPos = playerTransform.Position;
        var facing = _camera.Forward;

        foreach (var (entity, enemy, enemyHealth, enemyTransform) in QueryEnemies())
        {
            if (!enemyHealth.IsAlive) continue;

            var toEnemy = enemyTransform.Position - playerPos;
            float distance = toEnemy.Length();

            if (distance > playerCombat.AttackRange) continue;

            // Check if enemy is roughly in front of player
            if (toEnemy.LengthSquared() > 0.01f)
            {
                float dot = Vector3.Dot(Vector3.Normalize(toEnemy), facing);
                if (dot < 0.3f) continue; // Must be in front (within ~72 degree cone)
            }

            // Hit!
            enemyHealth.TakeDamage(playerCombat.EffectiveAttackDamage);

            if (!enemyHealth.IsAlive)
            {
                enemy.State = EnemyState.Dead;
                enemy.IsDead = true;
                enemy.DeathTimer = 3f; // Despawn after 3 seconds
            }
        }
    }

    private void UpdateTimers(CombatComponent combat, float dt)
    {
        combat.AttackTimer = MathF.Max(0, combat.AttackTimer - dt);
        combat.DodgeCooldownTimer = MathF.Max(0, combat.DodgeCooldownTimer - dt);
    }

    private IEnumerable<(Entity, PlayerComponent, CombatComponent, HealthComponent)> QueryPlayerCombat()
    {
        foreach (var (entity, player, combat) in _world.Query<PlayerComponent, CombatComponent>())
        {
            if (_world.HasComponent<HealthComponent>(entity))
                yield return (entity, player, combat, _world.GetComponent<HealthComponent>(entity));
        }
    }

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryEnemies()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
