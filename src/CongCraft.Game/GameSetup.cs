using System.Numerics;
using CongCraft.Engine.Audio;
using CongCraft.Engine.Core;
using CongCraft.Engine.Environment;
using CongCraft.Engine.Input;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using CongCraft.Engine.Combat;
using CongCraft.Engine.Dialogue;
using CongCraft.Engine.ECS;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.UI;
using CongCraft.Game.Systems;

namespace CongCraft.Game;

/// <summary>
/// Configures all game systems, creates the player entity, and launches the engine.
/// </summary>
public static class GameSetup
{
    public static void Run()
    {
        var engine = new GameEngine();

        // Create day/night cycle first (other systems reference it)
        var dayNight = new DayNightCycle();

        // Register services that systems need during Initialize
        engine.Services.Register(dayNight);

        // Register systems in logical order (execution order is by Priority)
        var inputSystem = new InputSystem();
        engine.RegisterSystem(inputSystem);
        engine.RegisterSystem(new PlayerMovementSystem());
        engine.RegisterSystem(dayNight);
        engine.RegisterSystem(new TerrainSystem(viewDistance: 2));
        engine.RegisterSystem(new VegetationPlacer());
        engine.RegisterSystem(new SkyRenderer());
        engine.RegisterSystem(new DialogueSystem());
        engine.RegisterSystem(new EquipmentSystem());
        engine.RegisterSystem(new CombatSystem());
        engine.RegisterSystem(new EnemyAISystem());
        engine.RegisterSystem(new InventorySystem());
        engine.RegisterSystem(new EnemySpawner());
        engine.RegisterSystem(new NpcSpawner());
        engine.RegisterSystem(new RenderSystem());
        engine.RegisterSystem(new EnemyRenderSystem());
        engine.RegisterSystem(new WaterPlane());
        engine.RegisterSystem(new HudSystem());
        engine.RegisterSystem(new AudioSystem());

        // Create player entity after engine loads (deferred via callback)
        SetupPlayerEntity(engine);

        engine.Run("CongCraft - Medieval RPG", 1280, 720);
    }

    private static void SetupPlayerEntity(GameEngine engine)
    {
        // Register a terrain generator service for player movement ground clamping
        var terrainGen = new TerrainGenerator();
        engine.Services.Register(terrainGen);

        // Player entity will be created on first frame via a setup system
        var setupSystem = new PlayerSetupSystem();
        engine.RegisterSystem(setupSystem);
    }
}

/// <summary>
/// One-shot system that creates the player entity on first frame.
/// Needs GL context to create the capsule mesh.
/// </summary>
internal sealed class PlayerSetupSystem : Engine.ECS.Systems.ISystem
{
    public int Priority => 5; // Before player movement

    private bool _created;
    private Engine.ECS.World _world = null!;
    private Silk.NET.OpenGL.GL _gl = null!;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<Silk.NET.OpenGL.GL>();
        _world = services.Get<Engine.ECS.World>();

        // Create player
        var player = _world.CreateEntity();
        _world.AddComponent(player, new TransformComponent
        {
            Position = new Vector3(0, 10, 0),
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

        // Inventory and equipment
        var inventory = new InventoryComponent();
        var startSword = ItemDatabase.Get("rusty_sword");
        if (startSword != null) inventory.TryAdd(startSword);
        var startPotion = ItemDatabase.Get("health_potion");
        if (startPotion != null) inventory.TryAdd(startPotion, 2);
        _world.AddComponent(player, inventory);
        _world.AddComponent(player, new EquipmentComponent());

        // Player capsule mesh
        var capsule = PrimitiveMeshBuilder.CreateCapsule(_gl, 0.3f, 1.8f, 12, 0.6f, 0.5f, 0.4f);
        var shader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _world.AddComponent(player, new MeshRendererComponent
        {
            Mesh = capsule,
            Shader = shader
        });
    }

    public void Update(GameTime time) { }
    public void Render(GameTime time) { }
    public void Dispose() { }
}
