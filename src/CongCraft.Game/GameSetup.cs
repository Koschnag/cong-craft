using System.Numerics;
using CongCraft.Engine.Audio;
using CongCraft.Engine.Core;
using CongCraft.Engine.Environment;
using CongCraft.Engine.Input;
using CongCraft.Engine.Level;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using CongCraft.Engine.Combat;
using CongCraft.Engine.Crafting;
using CongCraft.Engine.Dialogue;
using CongCraft.Engine.Dungeon;
using CongCraft.Engine.ECS;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using CongCraft.Engine.Magic;
using CongCraft.Engine.Quest;
using CongCraft.Engine.SaveLoad;
using CongCraft.Engine.UI;
using CongCraft.Engine.Boss;
using CongCraft.Engine.Weather;
using CongCraft.Engine.VFX;
using CongCraft.Game.Systems;

namespace CongCraft.Game;

/// <summary>
/// Configures all game systems, creates the player entity, and launches the engine.
/// Level 1: "Ashvale" — fixed, hand-designed terrain with placed objects.
/// </summary>
public static class GameSetup
{
    public static void Run()
    {
        var engine = new GameEngine();

        // === Load Level 1 data ===
        var levelData = LevelOneData.Create();
        var levelTerrainGen = new LevelTerrainGenerator(levelData);
        engine.Services.Register(levelData);
        engine.Services.Register(levelTerrainGen);

        // Legacy TerrainGenerator wrapper for systems that still reference it
        var terrainGen = new TerrainGenerator();
        engine.Services.Register(terrainGen);

        // Create day/night cycle first (other systems reference it)
        var dayNight = new DayNightCycle();
        engine.Services.Register(dayNight);

        // Register systems in logical order (execution order is by Priority)
        var inputSystem = new InputSystem();
        engine.RegisterSystem(inputSystem);
        engine.RegisterSystem(new WeatherSystem());
        engine.RegisterSystem(new PlayerMovementSystem());
        engine.RegisterSystem(dayNight);
        engine.RegisterSystem(new TerrainSystem(viewDistance: 3));
        engine.RegisterSystem(new VegetationPlacer());
        engine.RegisterSystem(new SkyRenderer());
        engine.RegisterSystem(new DialogueSystem());
        engine.RegisterSystem(new EquipmentSystem());
        engine.RegisterSystem(new CombatSystem());
        engine.RegisterSystem(new MagicSystem());
        engine.RegisterSystem(new EnemyAISystem());
        engine.RegisterSystem(new BossAISystem());
        engine.RegisterSystem(new InventorySystem());
        engine.RegisterSystem(new QuestSystem());
        engine.RegisterSystem(new CraftingSystem());
        engine.RegisterSystem(new LevelingSystem());
        engine.RegisterSystem(new SaveLoadSystem());
        engine.RegisterSystem(new DungeonSystem());
        engine.RegisterSystem(new EnemySpawner());
        engine.RegisterSystem(new NpcSpawner());
        engine.RegisterSystem(new TorchSystem());
        engine.RegisterSystem(new CombatVfxSystem());
        engine.RegisterSystem(new RenderSystem());
        engine.RegisterSystem(new EnemyRenderSystem());
        engine.RegisterSystem(new WaterPlane());
        engine.RegisterSystem(new ParticleRenderSystem());
        engine.RegisterSystem(new MenuSystem());
        engine.RegisterSystem(new TutorialSystem());
        engine.RegisterSystem(new HudSystem());
        engine.RegisterSystem(new AudioSystem());

        // Create player entity after engine loads (deferred via callback)
        SetupPlayerEntity(engine, levelData);

        engine.Run("CongCraft - Ashvale", 1920, 1080);
    }

    private static void SetupPlayerEntity(GameEngine engine, LevelData levelData)
    {
        // Player entity will be created on first frame via a setup system
        var setupSystem = new PlayerSetupSystem(levelData.PlayerSpawn);
        engine.RegisterSystem(setupSystem);
    }
}

/// <summary>
/// One-shot system that creates the player entity on first frame.
/// Also initializes the AssetManager and exports procedural meshes as OBJ files
/// so they can be replaced with AI-generated models.
/// </summary>
internal sealed class PlayerSetupSystem : Engine.ECS.Systems.ISystem
{
    public int Priority => 5; // Before player movement

    private readonly Vector3 _spawnPos;
    private Engine.ECS.World _world = null!;
    private Silk.NET.OpenGL.GL _gl = null!;

    public PlayerSetupSystem(Vector3 spawnPos)
    {
        _spawnPos = spawnPos;
    }

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<Silk.NET.OpenGL.GL>();
        _world = services.Get<Engine.ECS.World>();

        // Initialize asset manager — exports procedural meshes as OBJ files on first run.
        // Replace the OBJ files in assets/models/ with AI-generated models to upgrade quality.
        var assets = new AssetManager(_gl);
        services.Register(assets);
        assets.GenerateAllAssets();

        // Compute proper spawn height from terrain
        var terrainGen = services.Get<LevelTerrainGenerator>();
        float terrainY = terrainGen.GetHeightAt(_spawnPos.X, _spawnPos.Z);
        var spawnPos = new Vector3(_spawnPos.X, terrainY + 0.9f, _spawnPos.Z);

        // Initialize camera to player spawn immediately so scene renders correctly from frame 1
        var camera = services.Get<Engine.Rendering.Camera>();
        camera.Target = spawnPos;

        // Create player at level spawn point (clamped to terrain)
        var player = _world.CreateEntity();
        _world.AddComponent(player, new TransformComponent
        {
            Position = spawnPos,
            Scale = Vector3.One
        });
        _world.AddComponent(player, new PlayerComponent());
        _world.AddComponent(player, new HealthComponent { Current = 100, Max = 100 });
        _world.AddComponent(player, new CombatComponent
        {
            AttackDamage = 15f,
            AttackRange = 2.5f,
            AttackCooldown = 0.6f,
            BlockDamageReduction = 0.7f,
            DodgeDistance = 3f,
            DodgeCooldown = 0.8f,
            DodgeDuration = 0.3f
        });

        // Inventory and equipment — give the player standard starting gear
        var inventory = new InventoryComponent();
        foreach (var (itemId, qty) in new[]
        {
            ("rusty_sword", 1),
            ("leather_helm", 1),
            ("leather_chest", 1),
            ("iron_greaves", 1),
            ("health_potion", 3),
            ("mana_potion", 2),
            ("herb", 5),
            ("gold_coin", 25),
        })
        {
            var item = ItemDatabase.Get(itemId);
            if (item != null) inventory.TryAdd(item, qty);
        }
        _world.AddComponent(player, inventory);
        _world.AddComponent(player, new EquipmentComponent());
        _world.AddComponent(player, new QuestJournal());
        _world.AddComponent(player, new LevelComponent());
        _world.AddComponent(player, new SkillTree());
        _world.AddComponent(player, new ManaComponent());
        _world.AddComponent(player, new SpellState());

        // Player warrior mesh — load from OBJ asset or fall back to procedural
        var playerMesh = assets.LoadOrGenerate("player_warrior", HighResPlayerMeshBuilder.GenerateData);
        var shader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _world.AddComponent(player, new MeshRendererComponent
        {
            Mesh = playerMesh,
            Shader = shader
        });
    }

    public void Update(GameTime time) { }
    public void Render(GameTime time) { }
    public void Dispose() { }
}
