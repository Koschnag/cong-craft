using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Level;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Spawns enemies at fixed positions defined by the level data.
/// Each enemy zone has a specific type, count, and patrol area.
/// </summary>
public sealed class EnemySpawner : ISystem
{
    public int Priority => 46; // After vegetation

    private GL _gl = null!;
    private World _world = null!;
    private LevelTerrainGenerator _levelGen = null!;
    private LevelData? _levelData;
    private AssetManager? _assets;
    private Mesh _enemyMesh = null!;
    private Mesh _skeletonMesh = null!;
    private Mesh _wolfMesh = null!;
    private Mesh _trollMesh = null!;
    private Shader _basicShader = null!;
    private MaterialTextures? _materialTextures;
    private bool _initialSpawnDone;

    // Respawn tracking
    private readonly List<SpawnPoint> _spawnPoints = new();
    private const float RespawnDelay = 45f;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _levelGen = services.Get<LevelTerrainGenerator>();
        services.TryGet(out _levelData);
        services.TryGet(out _assets);

        // Load from OBJ assets if available, otherwise fall back to procedural
        _enemyMesh = _assets?.LoadOrGenerate("enemy_bandit", HighResEnemyMeshBuilder.GenerateData)
            ?? HighResEnemyMeshBuilder.Create(_gl);
        _skeletonMesh = _assets?.LoadOrGenerate("enemy_skeleton", HighResEnemyMeshBuilder.GenerateSkeletonData)
            ?? HighResEnemyMeshBuilder.CreateSkeleton(_gl);
        _wolfMesh = _assets?.LoadOrGenerate("enemy_wolf", HighResEnemyMeshBuilder.GenerateWolfData)
            ?? HighResEnemyMeshBuilder.CreateWolf(_gl);
        _trollMesh = _assets?.LoadOrGenerate("enemy_troll", HighResEnemyMeshBuilder.GenerateTrollData)
            ?? HighResEnemyMeshBuilder.CreateTroll(_gl);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _materialTextures = services.Get<MaterialTextures>();
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
        if (_levelData?.EnemyZones != null)
        {
            // Use fixed level data enemy zones
            foreach (var zone in _levelData.EnemyZones)
            {
                var type = ParseEnemyType(zone.EnemyType);
                SpawnGroup(zone.CenterX, zone.CenterZ, zone.Radius, zone.Count, type);
            }
        }
        else
        {
            // Fallback: default spawn locations
            SpawnGroup(30f, 30f, 10f, 3, EnemyType.Wolf);
            SpawnGroup(-20f, -50f, 10f, 2, EnemyType.Wolf);
            SpawnGroup(-40f, 20f, 10f, 2, EnemyType.Bandit);
            SpawnGroup(50f, -30f, 10f, 3, EnemyType.Bandit);
            SpawnGroup(60f, 60f, 10f, 2, EnemyType.Skeleton);
            SpawnGroup(-60f, -40f, 10f, 2, EnemyType.Skeleton);
            SpawnGroup(80f, 0f, 5f, 1, EnemyType.Troll);
        }
    }

    private void SpawnGroup(float centerX, float centerZ, float radius, int count, EnemyType type)
    {
        // Deterministic placement using golden angle spiral
        float goldenAngle = MathF.PI * (3f - MathF.Sqrt(5f));
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? (float)i / (count - 1) : 0f;
            float r = radius * 0.3f + radius * 0.7f * MathF.Sqrt(t);
            float angle = i * goldenAngle;
            float x = centerX + r * MathF.Cos(angle);
            float z = centerZ + r * MathF.Sin(angle);
            float height = _levelGen.GetHeightAt(x, z);

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
        var mesh = type switch
        {
            EnemyType.Skeleton => _skeletonMesh,
            EnemyType.Wolf => _wolfMesh,
            EnemyType.Troll => _trollMesh,
            _ => _enemyMesh
        };
        _world.AddComponent(entity, new MeshRendererComponent
        {
            Mesh = mesh,
            Shader = _basicShader
        });

        return entity;
    }

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

    private static EnemyType ParseEnemyType(string s) => s.ToLowerInvariant() switch
    {
        "wolf" => EnemyType.Wolf,
        "bandit" => EnemyType.Bandit,
        "skeleton" => EnemyType.Skeleton,
        "troll" => EnemyType.Troll,
        _ => EnemyType.Bandit
    };

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _enemyMesh.Dispose();
        _skeletonMesh.Dispose();
        _wolfMesh.Dispose();
        _trollMesh.Dispose();
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
