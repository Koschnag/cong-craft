using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.VFX;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Dungeon;

/// <summary>
/// Manages dungeon state: entering, rendering, spawning enemies, and exiting.
/// Press G near the dungeon entrance marker to enter. Press G at exit to leave.
/// </summary>
public sealed class DungeonSystem : ISystem
{
    public int Priority => 30; // After quest system

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _dungeonShader = null!;
    private ShadowMap? _shadowMap;
    private MaterialTextures? _materialTextures;

    private DungeonLayout? _currentLayout;
    private Mesh? _floorMesh;
    private Mesh? _wallMesh;
    private Mesh? _ceilingMesh;

    private bool _inDungeon;
    private Vector3 _savedOverworldPos;
    private Vector3 _savedSunDirection;
    private Vector3 _savedAmbient;
    private float _savedFogDensity;

    // Dungeon entrance location in overworld
    private readonly Vector3 _entranceOverworld = new(25f, 0, 25f);
    private const float EntranceRadius = 3f;

    // Torch emitters in dungeon
    private readonly List<ParticleEmitter> _dungeonTorchEmitters = new();
    private readonly List<Entity> _dungeonEnemies = new();
    private float _flickerTimer;
    private readonly Random _rng = new(54321);

    public bool InDungeon => _inDungeon;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _dungeonShader = new Shader(_gl, ShaderSources.BasicFragmentPointLights.Contains("uPointLightCount")
            ? ShaderSources.BasicVertexPointLights
            : ShaderSources.BasicVertex,
            ShaderSources.BasicFragmentPointLights.Contains("uPointLightCount")
            ? ShaderSources.BasicFragmentPointLights
            : ShaderSources.BasicFragment);

        _shadowMap = services.Get<ShadowMap>();
        _materialTextures = services.Get<MaterialTextures>();
        services.Register(this);
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;
        _flickerTimer += dt;

        if (_inDungeon)
        {
            // Emit torch particles
            foreach (var emitter in _dungeonTorchEmitters)
                emitter.Emit(1);

            // Check for exit: press G near exit tile
            if (input.IsKeyPressed(Key.G))
                TryExitDungeon();

            // Keep player within dungeon bounds
            ClampPlayerToDungeon();
        }
        else
        {
            // Check for entrance: press G near entrance marker
            if (input.IsKeyPressed(Key.G))
                TryEnterDungeon();
        }
    }

    private void TryEnterDungeon()
    {
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(
                new Vector3(transform.Position.X, 0, transform.Position.Z),
                new Vector3(_entranceOverworld.X, 0, _entranceOverworld.Z));

            if (dist > EntranceRadius) continue;

            // Save overworld state
            _savedOverworldPos = transform.Position;
            _savedSunDirection = _lighting.SunDirection;
            _savedAmbient = _lighting.AmbientColor;
            _savedFogDensity = _lighting.FogDensity;

            // Generate dungeon
            var generator = new DungeonGenerator(seed: 42);
            _currentLayout = generator.Generate(6);

            // Build meshes
            _floorMesh?.Dispose();
            _wallMesh?.Dispose();
            _ceilingMesh?.Dispose();
            _floorMesh = DungeonMeshBuilder.CreateFloor(_gl, _currentLayout);
            _wallMesh = DungeonMeshBuilder.CreateWalls(_gl, _currentLayout);
            _ceilingMesh = DungeonMeshBuilder.CreateCeiling(_gl, _currentLayout);

            // Move player to dungeon entrance
            var entrance = _currentLayout.Entrance;
            transform.Position = new Vector3(
                entrance.X * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2,
                1f,
                entrance.Y * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2);

            // Set dungeon lighting: dark, no sun
            _lighting.SunDirection = new Vector3(0, -1, 0);
            _lighting.SunIntensity = 0.05f;
            _lighting.AmbientColor = new Vector3(0.03f, 0.03f, 0.05f);
            _lighting.FogDensity = 0.03f;
            _lighting.FogColor = new Vector3(0.02f, 0.02f, 0.03f);

            // Spawn dungeon torches
            SpawnDungeonTorches();

            // Spawn dungeon enemies
            SpawnDungeonEnemies();

            _inDungeon = true;
            break;
        }
    }

    private void TryExitDungeon()
    {
        if (_currentLayout == null) return;

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            var exit = _currentLayout.Exit;
            float exitX = exit.X * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;
            float exitZ = exit.Y * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;

            float dist = Vector3.Distance(
                new Vector3(transform.Position.X, 0, transform.Position.Z),
                new Vector3(exitX, 0, exitZ));

            if (dist > EntranceRadius) continue;

            // Restore overworld state
            transform.Position = _savedOverworldPos;
            _lighting.SunDirection = _savedSunDirection;
            _lighting.AmbientColor = _savedAmbient;
            _lighting.FogDensity = _savedFogDensity;
            _lighting.SunIntensity = 1f;

            // Cleanup
            CleanupDungeon();
            _inDungeon = false;
            break;
        }
    }

    private void SpawnDungeonTorches()
    {
        if (_currentLayout == null) return;

        // Place torches in room centers
        foreach (var room in _currentLayout.Rooms)
        {
            float wx = room.Center.X * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;
            float wz = room.Center.Y * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;

            var config = VfxPresets.TorchFire;
            config.Position = new Vector3(wx, DungeonMeshBuilder.WallHeight - 0.5f, wz);
            var emitter = new ParticleEmitter(config, _rng.Next());
            _dungeonTorchEmitters.Add(emitter);
            ParticleRenderSystem.RegisterEmitter(emitter);
        }
    }

    private void SpawnDungeonEnemies()
    {
        if (_currentLayout == null) return;

        // Spawn enemies in each room except entrance
        for (int i = 1; i < _currentLayout.Rooms.Count; i++)
        {
            var room = _currentLayout.Rooms[i];
            int enemyCount = _rng.Next(1, 3);

            for (int e = 0; e < enemyCount; e++)
            {
                float rx = (room.X + _rng.Next(1, room.Width - 1)) * DungeonMeshBuilder.TileSize;
                float rz = (room.Y + _rng.Next(1, room.Height - 1)) * DungeonMeshBuilder.TileSize;

                var entity = _world.CreateEntity();
                _world.AddComponent(entity, new TransformComponent
                {
                    Position = new Vector3(rx, 0.7f, rz),
                    Scale = Vector3.One * 1.2f // Slightly larger dungeon enemies
                });
                _world.AddComponent(entity, new EnemyComponent
                {
                    State = EnemyState.Patrol,
                    DetectionRange = 10f,
                    AttackRange = 2f,
                    MoveSpeed = 2f,
                    ChaseSpeed = 3.5f,
                    PatrolRadius = room.Width * DungeonMeshBuilder.TileSize * 0.4f,
                    SpawnPosition = new Vector3(rx, 0.7f, rz),
                    PatrolTarget = new Vector3(rx, 0.7f, rz)
                });
                _world.AddComponent(entity, new HealthComponent { Current = 80, Max = 80 }); // Tougher
                _world.AddComponent(entity, new CombatComponent
                {
                    AttackDamage = 12f,
                    AttackRange = 2f,
                    AttackCooldown = 1f
                });

                _dungeonEnemies.Add(entity);
            }
        }
    }

    private void ClampPlayerToDungeon()
    {
        if (_currentLayout == null) return;

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            // Keep Y at dungeon floor level
            var pos = transform.Position;
            pos.Y = 1f;

            // Clamp to dungeon bounds
            float maxX = _currentLayout.Width * DungeonMeshBuilder.TileSize;
            float maxZ = _currentLayout.Height * DungeonMeshBuilder.TileSize;
            pos.X = MathF.Max(0, MathF.Min(maxX, pos.X));
            pos.Z = MathF.Max(0, MathF.Min(maxZ, pos.Z));

            transform.Position = pos;
        }
    }

    private void CleanupDungeon()
    {
        // Remove dungeon torches
        foreach (var emitter in _dungeonTorchEmitters)
            ParticleRenderSystem.UnregisterEmitter(emitter);
        _dungeonTorchEmitters.Clear();

        // Remove dungeon enemies
        foreach (var entity in _dungeonEnemies)
        {
            if (_world.HasEntity(entity))
                _world.DestroyEntity(entity);
        }
        _dungeonEnemies.Clear();

        _floorMesh?.Dispose();
        _wallMesh?.Dispose();
        _ceilingMesh?.Dispose();
        _floorMesh = null;
        _wallMesh = null;
        _ceilingMesh = null;
        _currentLayout = null;
    }

    public void Render(GameTime time)
    {
        if (!_inDungeon || _floorMesh == null) return;

        _dungeonShader.Use();
        _dungeonShader.SetUniform("uView", _camera.ViewMatrix);
        _dungeonShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _dungeonShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_dungeonShader);
        _shadowMap?.BindToShader(_dungeonShader, 0);
        _materialTextures?.BindToShader(_dungeonShader);

        // Apply point lights if available
        if (_world.TryGetSingleton<InputState>(out _)) // Just checking world is valid
        {
            // Point light data is managed by TorchSystem for overworld
            // In dungeon, we set our own
            _dungeonShader.SetUniform("uPointLightCount", Math.Min(_currentLayout?.Rooms.Count ?? 0, 4));
            for (int i = 0; i < Math.Min(_currentLayout?.Rooms.Count ?? 0, 4); i++)
            {
                var room = _currentLayout!.Rooms[i];
                float wx = room.Center.X * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;
                float wz = room.Center.Y * DungeonMeshBuilder.TileSize + DungeonMeshBuilder.TileSize / 2;
                float flicker = 1f + MathF.Sin(_flickerTimer * (4f + i)) * 0.2f;

                _dungeonShader.SetUniform($"uPointLightPos[{i}]", new Vector3(wx, 3f, wz));
                _dungeonShader.SetUniform($"uPointLightColor[{i}]", new Vector3(1f, 0.6f, 0.2f));
                _dungeonShader.SetUniform($"uPointLightIntensity[{i}]", 2f * flicker);
                _dungeonShader.SetUniform($"uPointLightRadius[{i}]", 12f);
            }
        }

        var model = Matrix4x4.Identity;
        _dungeonShader.SetUniform("uModel", model);

        _floorMesh.Draw();
        _wallMesh?.Draw();
        _ceilingMesh?.Draw();
    }

    public void Dispose()
    {
        CleanupDungeon();
        _dungeonShader.Dispose();
    }
}
