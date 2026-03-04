using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Terrain;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Controls enemy behavior: patrol, chase player, attack, and death.
/// </summary>
public sealed class EnemyAISystem : ISystem
{
    public int Priority => 20; // After combat, before physics

    private World _world = null!;
    private TerrainGenerator _terrainGen = null!;
    private Random _rng = new(12345);

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
    }

    public void Update(GameTime time)
    {
        float dt = time.DeltaTimeF;

        // Find player position
        Vector3 playerPos = Vector3.Zero;
        Entity playerEntity = default;
        CombatComponent? playerCombat = null;
        HealthComponent? playerHealth = null;

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            playerEntity = entity;
            if (_world.HasComponent<CombatComponent>(entity))
                playerCombat = _world.GetComponent<CombatComponent>(entity);
            if (_world.HasComponent<HealthComponent>(entity))
                playerHealth = _world.GetComponent<HealthComponent>(entity);
            break;
        }

        // Process each enemy
        var deadEntities = new List<Entity>();

        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);

            // Handle death
            if (enemy.State == EnemyState.Dead)
            {
                enemy.DeathTimer -= dt;
                if (enemy.DeathTimer <= 0)
                    deadEntities.Add(entity);
                continue;
            }

            if (!health.IsAlive)
            {
                enemy.State = EnemyState.Dead;
                enemy.DeathTimer = 3f;
                continue;
            }

            float distToPlayer = Vector3.Distance(transform.Position, playerPos);

            // State transitions
            switch (enemy.State)
            {
                case EnemyState.Patrol:
                    if (distToPlayer < enemy.DetectionRange)
                    {
                        enemy.State = EnemyState.Chase;
                        break;
                    }
                    UpdatePatrol(enemy, transform, dt);
                    break;

                case EnemyState.Chase:
                    if (distToPlayer > enemy.DetectionRange * 1.5f)
                    {
                        enemy.State = EnemyState.Patrol;
                        enemy.PatrolTarget = enemy.SpawnPosition;
                        break;
                    }
                    if (distToPlayer < enemy.AttackRange)
                    {
                        enemy.State = EnemyState.Attack;
                        break;
                    }
                    UpdateChase(enemy, transform, playerPos, dt);
                    break;

                case EnemyState.Attack:
                    if (distToPlayer > enemy.AttackRange * 1.5f)
                    {
                        enemy.State = EnemyState.Chase;
                        break;
                    }
                    UpdateAttack(enemy, transform, playerPos, playerCombat, playerHealth, dt);
                    break;
            }

            // Ground clamping
            var pos = transform.Position;
            pos.Y = _terrainGen.GetHeightAt(pos.X, pos.Z) + 0.7f;
            transform.Position = pos;

            // Face movement direction (toward player when chasing/attacking)
            if (enemy.State is EnemyState.Chase or EnemyState.Attack)
            {
                var dir = playerPos - transform.Position;
                if (dir.LengthSquared() > 0.01f)
                {
                    float yaw = MathF.Atan2(dir.X, dir.Z);
                    transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
                }
            }
        }

        // Clean up dead enemies
        foreach (var entity in deadEntities)
        {
            _world.DestroyEntity(entity);
        }
    }

    private void UpdatePatrol(EnemyComponent enemy, TransformComponent transform, float dt)
    {
        var toTarget = enemy.PatrolTarget - transform.Position;
        toTarget.Y = 0;

        if (toTarget.LengthSquared() < 1f)
        {
            // Reached patrol point, wait
            enemy.PatrolWaitTimer -= dt;
            if (enemy.PatrolWaitTimer <= 0)
            {
                // Pick new patrol point
                float angle = (float)(_rng.NextDouble() * MathF.Tau);
                float dist = (float)(_rng.NextDouble() * enemy.PatrolRadius);
                enemy.PatrolTarget = enemy.SpawnPosition + new Vector3(
                    MathF.Cos(angle) * dist, 0, MathF.Sin(angle) * dist);
                enemy.PatrolWaitTimer = enemy.PatrolWaitDuration;
            }
            return;
        }

        // Move toward patrol target
        var dir = Vector3.Normalize(new Vector3(toTarget.X, 0, toTarget.Z));
        transform.Position += dir * enemy.MoveSpeed * dt;

        // Face movement direction
        float yaw = MathF.Atan2(dir.X, dir.Z);
        transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
    }

    private void UpdateChase(EnemyComponent enemy, TransformComponent transform,
        Vector3 playerPos, float dt)
    {
        var dir = playerPos - transform.Position;
        dir.Y = 0;
        if (dir.LengthSquared() > 0.01f)
        {
            dir = Vector3.Normalize(dir);
            transform.Position += dir * enemy.ChaseSpeed * dt;
        }
    }

    private void UpdateAttack(EnemyComponent enemy, TransformComponent transform,
        Vector3 playerPos, CombatComponent? playerCombat, HealthComponent? playerHealth, float dt)
    {
        enemy.AttackTimer -= dt;
        if (enemy.AttackTimer > 0) return;

        // Attack!
        enemy.AttackTimer = enemy.AttackCooldown;

        if (playerHealth == null || !playerHealth.IsAlive) return;

        float damage = 8f; // Base enemy damage

        // Check if player is blocking
        if (playerCombat != null && playerCombat.IsBlocking)
        {
            damage *= (1f - playerCombat.BlockDamageReduction);
        }

        // Check if player is dodging (invulnerable)
        if (playerCombat != null && playerCombat.IsDodging)
        {
            damage = 0;
        }

        // Magic shield reduces damage
        if (playerCombat != null && playerCombat.MagicShieldActive)
        {
            damage *= (1f - playerCombat.MagicShieldReduction);
        }

        if (damage > 0)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
