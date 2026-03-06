using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Spawns enemies on the terrain with variety and respawning.
/// Different enemy types have distinct stats and behaviors.
/// </summary>
public sealed class EnemySpawner : ISystem
{
    public int Priority => 46; // After vegetation

    private GL _gl = null!;
    private World _world = null!;
    private TerrainGenerator _terrainGen = null!;
    private Mesh _enemyMesh = null!;
    private Mesh _skeletonMesh = null!;
    private Mesh _swordMesh = null!;
    private Shader _basicShader = null!;
    private bool _initialSpawnDone;

    // Respawn tracking
    private readonly List<SpawnPoint> _spawnPoints = new();
    private const float RespawnDelay = 45f;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _enemyMesh = EnemyMeshBuilder.Create(_gl);
        _skeletonMesh = EnemyMeshBuilder.CreateSkeleton(_gl);
        _swordMesh = SwordMeshBuilder.Create(_gl);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time)
    {
        if (!_initialSpawnDone)
        {
            _initialSpawnDone = true;
            InitialSpawn();
        }

        float dt = time.DeltaTimeF;

        // Mark spawn points as inactive when their enemy dies
        foreach (var sp in _spawnPoints)
        {
            if (!sp.IsActive) continue;
            if (!_world.HasComponent<HealthComponent>(sp.Entity)) continue;
            var health = _world.GetComponent<HealthComponent>(sp.Entity);
            if (!health.IsAlive)
            {
                sp.IsActive = false;
                sp.RespawnTimer = RespawnDelay;
            }
        }

        // Check for respawns
        foreach (var sp in _spawnPoints)
        {
            if (sp.IsActive) continue;

            sp.RespawnTimer -= dt;
            if (sp.RespawnTimer <= 0)
            {
                sp.Entity = SpawnEnemy(sp.Position, sp.EnemyType);
                sp.IsActive = true;
            }
        }
    }

    private void InitialSpawn()
    {
        var rng = new Random(54321);

        // Wolf packs — fast, weak, near start
        SpawnGroup(rng, 30f, 30f, 3, EnemyType.Wolf);
        SpawnGroup(rng, -20f, -50f, 2, EnemyType.Wolf);

        // Bandits — balanced, mid-range
        SpawnGroup(rng, -40f, 20f, 2, EnemyType.Bandit);
        SpawnGroup(rng, 50f, -30f, 3, EnemyType.Bandit);

        // Skeletons — heavy hitters, further out
        SpawnGroup(rng, 60f, 60f, 2, EnemyType.Skeleton);
        SpawnGroup(rng, -60f, -40f, 2, EnemyType.Skeleton);

        // Troll — mini-boss, one dangerous encounter
        SpawnGroup(rng, 80f, 0f, 1, EnemyType.Troll);
    }

    private void SpawnGroup(Random rng, float centerX, float centerZ, int count, EnemyType type)
    {
        for (int i = 0; i < count; i++)
        {
            float x = centerX + (float)(rng.NextDouble() * 10 - 5);
            float z = centerZ + (float)(rng.NextDouble() * 10 - 5);
            float height = _terrainGen.GetHeightAt(x, z);

            if (height < 2f) continue;

            var spawnPos = new Vector3(x, height + 0.7f, z);
            var entity = SpawnEnemy(spawnPos, type);

            _spawnPoints.Add(new SpawnPoint
            {
                Position = spawnPos,
                EnemyType = type,
                Entity = entity,
                IsActive = true
            });
        }
    }

    private Entity SpawnEnemy(Vector3 spawnPos, EnemyType type)
    {
        var (hp, damage, range, moveSpeed, chaseSpeed, detectRange, attackCooldown, patrolRadius) =
            GetEnemyStats(type);

        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TransformComponent
        {
            Position = spawnPos,
            Scale = type == EnemyType.Troll ? Vector3.One * 1.5f : Vector3.One
        });
        _world.AddComponent(entity, new EnemyComponent
        {
            Type = type,
            SpawnPosition = spawnPos,
            PatrolTarget = spawnPos,
            MoveSpeed = moveSpeed,
            ChaseSpeed = chaseSpeed,
            DetectionRange = detectRange,
            AttackRange = range,
            AttackCooldown = attackCooldown,
            PatrolRadius = patrolRadius
        });
        _world.AddComponent(entity, new HealthComponent
        {
            Current = hp,
            Max = hp
        });
        _world.AddComponent(entity, new CombatComponent
        {
            AttackDamage = damage,
            AttackRange = range
        });
        _world.AddComponent(entity, new MeshRendererComponent
        {
            Mesh = type == EnemyType.Skeleton ? _skeletonMesh : _enemyMesh,
            Shader = _basicShader
        });

        return entity;
    }

    /// <summary>
    /// Returns stats tuple for each enemy type.
    /// </summary>
    internal static (float hp, float damage, float range, float moveSpeed, float chaseSpeed,
        float detectRange, float attackCooldown, float patrolRadius) GetEnemyStats(EnemyType type)
    {
        return type switch
        {
            EnemyType.Wolf     => (35f,  6f,  2f,   3.5f, 5.5f, 18f, 0.8f, 12f),
            EnemyType.Bandit   => (60f,  10f, 2.5f, 2.5f, 4f,   15f, 1.2f, 8f),
            EnemyType.Skeleton => (80f,  15f, 2.5f, 1.5f, 3f,   12f, 1.8f, 6f),
            EnemyType.Troll    => (200f, 25f, 3f,   1f,   2.5f, 20f, 2.5f, 5f),
            _                  => (50f,  8f,  2f,   2.5f, 4f,   15f, 1.2f, 10f)
        };
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _enemyMesh.Dispose();
        _skeletonMesh.Dispose();
        _swordMesh.Dispose();
        _basicShader.Dispose();
    }

    private sealed class SpawnPoint
    {
        public Vector3 Position;
        public EnemyType EnemyType;
        public Entity Entity;
        public bool IsActive;
        public float RespawnTimer;
    }
}
