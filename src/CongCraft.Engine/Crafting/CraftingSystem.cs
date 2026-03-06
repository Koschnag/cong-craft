using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Crafting;

/// <summary>
/// Manages crafting stations in the world and handles crafting interactions.
/// Press C near a station to open crafting, Up/Down to select, Enter to craft, Escape to close.
/// </summary>
public sealed class CraftingSystem : ISystem
{
    public int Priority => 27; // After inventory

    private World _world = null!;
    private GL _gl = null!;
    private TerrainGenerator _terrainGen = null!;
    private Mesh _stationMesh = null!;
    private Shader _basicShader = null!;
    private MaterialTextures? _materialTextures;
    private bool _stationsSpawned;

    // Station placement positions (near NPCs)
    private static readonly (float X, float Z, CraftingStationType Type)[] StationPositions =
    {
        (10f, 12f, CraftingStationType.Anvil),       // Near blacksmith (8, 12)
        (-3f, 15f, CraftingStationType.Alchemy),     // Near elder (-5, 15)
        (14f, 5f, CraftingStationType.Workbench),    // Near merchant (12, 5)
    };

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _materialTextures = services.Get<MaterialTextures>();
        _stationMesh = PrimitiveMeshBuilder.CreateCube(_gl, 0.8f, 0.55f, 0.4f, 0.25f);

        _world.SetSingleton(new CraftingState());
    }

    public void Update(GameTime time)
    {
        if (!_stationsSpawned)
        {
            _stationsSpawned = true;
            SpawnStations();
        }

        var input = _world.GetSingleton<InputState>();
        var craftState = _world.GetSingleton<CraftingState>();

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            if (!_world.HasComponent<InventoryComponent>(entity)) continue;
            var inventory = _world.GetComponent<InventoryComponent>(entity);

            if (craftState.IsOpen)
            {
                HandleCraftingInput(input, craftState, inventory);
            }
            else
            {
                // Check for C key to open crafting near a station
                if (input.IsKeyPressed(Key.C))
                    TryOpenCrafting(transform.Position, craftState);
            }
        }
    }

    private void TryOpenCrafting(Vector3 playerPos, CraftingState state)
    {
        foreach (var (entity, station, transform) in _world.Query<CraftingComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist > station.InteractionRadius) continue;

            state.IsOpen = true;
            state.StationType = station.StationType;
            state.SelectedRecipe = 0;
            state.AvailableRecipes = CraftingDatabase.GetByStation(station.StationType).ToList();
            return;
        }
    }

    private void HandleCraftingInput(InputState input, CraftingState state, InventoryComponent inventory)
    {
        if (input.IsKeyPressed(Key.Escape))
        {
            state.IsOpen = false;
            return;
        }

        if (state.AvailableRecipes.Count == 0) return;

        if (input.IsKeyPressed(Key.Up) && state.SelectedRecipe > 0)
            state.SelectedRecipe--;
        if (input.IsKeyPressed(Key.Down) && state.SelectedRecipe < state.AvailableRecipes.Count - 1)
            state.SelectedRecipe++;

        if (input.IsKeyPressed(Key.Enter))
        {
            var recipe = state.AvailableRecipes[state.SelectedRecipe];
            TryCraft(recipe, inventory);
        }
    }

    private void SpawnStations()
    {
        foreach (var (x, z, type) in StationPositions)
        {
            float height = _terrainGen.GetHeightAt(x, z);
            if (height < 2f) height = 2f;

            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = new Vector3(x, height + 0.4f, z),
                Scale = new Vector3(1.2f, 0.8f, 1.2f)
            });
            _world.AddComponent(entity, new CraftingComponent
            {
                StationType = type,
                InteractionRadius = 3f
            });
            _world.AddComponent(entity, new MeshRendererComponent
            {
                Mesh = _stationMesh,
                Shader = _basicShader
            });
        }
    }

    /// <summary>
    /// Checks if all ingredients are available in the inventory.
    /// </summary>
    public static bool CanCraft(CraftingRecipe recipe, InventoryComponent inventory)
    {
        foreach (var ingredient in recipe.Ingredients)
        {
            if (inventory.CountOf(ingredient.ItemId) < ingredient.Count)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to craft an item. Removes ingredients and adds output to inventory.
    /// </summary>
    public static bool TryCraft(CraftingRecipe recipe, InventoryComponent inventory)
    {
        if (!CanCraft(recipe, inventory)) return false;

        var outputItem = ItemDatabase.Get(recipe.OutputItemId);
        if (outputItem == null) return false;

        // Remove ingredients
        foreach (var ingredient in recipe.Ingredients)
            inventory.TryRemove(ingredient.ItemId, ingredient.Count);

        // Add crafted item
        inventory.TryAdd(outputItem, recipe.OutputCount);
        return true;
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _stationMesh.Dispose();
        _basicShader.Dispose();
    }
}
