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
/// Spawns enemies on the terrain at startup. Places them in groups.
/// </summary>
public sealed class EnemySpawner : ISystem
{
    public int Priority => 46; // After vegetation

    private GL _gl = null!;
    private World _world = null!;
    private TerrainGenerator _terrainGen = null!;
    private Mesh _enemyMesh = null!;
    private Mesh _swordMesh = null!;
    private Shader _basicShader = null!;
    private bool _spawned;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _enemyMesh = EnemyMeshBuilder.Create(_gl);
        _swordMesh = SwordMeshBuilder.Create(_gl);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time)
    {
        if (_spawned) return;
        _spawned = true;

        var rng = new Random(54321);

        // Spawn groups of enemies around the map
        SpawnGroup(rng, 30f, 30f, 3);
        SpawnGroup(rng, -40f, 20f, 2);
        SpawnGroup(rng, 50f, -30f, 4);
        SpawnGroup(rng, -20f, -50f, 2);
        SpawnGroup(rng, 60f, 60f, 3);
    }

    private void SpawnGroup(Random rng, float centerX, float centerZ, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = centerX + (float)(rng.NextDouble() * 10 - 5);
            float z = centerZ + (float)(rng.NextDouble() * 10 - 5);
            float height = _terrainGen.GetHeightAt(x, z);

            // Don't spawn underwater or on very steep terrain
            if (height < 2f) continue;

            var spawnPos = new Vector3(x, height + 0.7f, z);

            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = spawnPos,
                Scale = Vector3.One
            });
            _world.AddComponent(entity, new EnemyComponent
            {
                SpawnPosition = spawnPos,
                PatrolTarget = spawnPos,
                PatrolWaitTimer = (float)rng.NextDouble() * 3f
            });
            _world.AddComponent(entity, new HealthComponent
            {
                Current = 50f,
                Max = 50f
            });
            _world.AddComponent(entity, new CombatComponent
            {
                AttackDamage = 8f,
                AttackRange = 2f
            });
            _world.AddComponent(entity, new MeshRendererComponent
            {
                Mesh = _enemyMesh,
                Shader = _basicShader
            });
        }
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _enemyMesh.Dispose();
        _swordMesh.Dispose();
        _basicShader.Dispose();
    }
}
