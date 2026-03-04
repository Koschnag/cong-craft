using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Inventory;

/// <summary>
/// Handles item pickup from loot drops, inventory toggle, item use, and equipping.
/// Also spawns loot drops when enemies die.
/// </summary>
public sealed class InventorySystem : ISystem
{
    public int Priority => 25; // After combat and AI

    private World _world = null!;
    private GL _gl = null!;
    private TerrainGenerator _terrainGen = null!;
    private Mesh _lootMesh = null!;
    private Shader _basicShader = null!;
    private Random _rng = new(99999);

    // Track which enemies already dropped loot (by entity ID)
    private readonly HashSet<int> _lootedEnemies = new();

    // Loot tables: what items enemies can drop
    private static readonly (string itemId, float chance)[] EnemyLootTable =
    {
        ("gold_coin", 0.9f),
        ("health_potion", 0.4f),
        ("rusty_sword", 0.15f),
        ("iron_sword", 0.08f),
        ("leather_helm", 0.12f),
        ("chain_mail", 0.06f),
        ("iron_greaves", 0.08f),
        ("wolf_pelt", 0.3f),
    };

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _lootMesh = PrimitiveMeshBuilder.CreateCube(_gl, 0.3f, 0.9f, 0.8f, 0.2f);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;

        // Spawn loot for dying enemies
        SpawnEnemyLoot();

        // Animate loot drops (bobbing)
        foreach (var (entity, loot, transform) in _world.Query<LootDropComponent, TransformComponent>())
        {
            loot.BobTimer += dt * 2f;
            transform.Position = new Vector3(
                transform.Position.X,
                loot.SpawnY + MathF.Sin(loot.BobTimer) * 0.15f,
                transform.Position.Z);
        }

        // Handle player inventory interactions
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            if (!_world.HasComponent<InventoryComponent>(entity)) continue;
            var inventory = _world.GetComponent<InventoryComponent>(entity);

            // Toggle inventory with I
            if (input.IsKeyPressed(Key.I))
                inventory.IsOpen = !inventory.IsOpen;

            // Pick up nearby loot with F
            if (input.IsKeyPressed(Key.F))
                TryPickupNearby(entity, transform.Position, inventory);

            // Use health potion with H
            if (input.IsKeyPressed(Key.H))
                TryUseHealthPotion(entity, inventory);

            // Equip items with number keys when inventory is open
            if (inventory.IsOpen)
            {
                if (input.IsKeyPressed(Key.Number1)) TryEquipAt(entity, inventory, 0);
                if (input.IsKeyPressed(Key.Number2)) TryEquipAt(entity, inventory, 1);
                if (input.IsKeyPressed(Key.Number3)) TryEquipAt(entity, inventory, 2);
                if (input.IsKeyPressed(Key.Number4)) TryEquipAt(entity, inventory, 3);
                if (input.IsKeyPressed(Key.Number5)) TryEquipAt(entity, inventory, 4);

                // Navigate selection
                if (input.IsKeyPressed(Key.Up) && inventory.SelectedSlot > 0)
                    inventory.SelectedSlot--;
                if (input.IsKeyPressed(Key.Down) && inventory.SelectedSlot < inventory.Items.Count - 1)
                    inventory.SelectedSlot++;
            }
        }
    }

    private void SpawnEnemyLoot()
    {
        foreach (var (entity, enemy, health, transform) in QueryDeadEnemies())
        {
            if (_lootedEnemies.Contains(entity.Id)) continue;
            _lootedEnemies.Add(entity.Id);

            // Roll loot
            foreach (var (itemId, chance) in EnemyLootTable)
            {
                if (_rng.NextDouble() > chance) continue;

                var itemData = ItemDatabase.Get(itemId);
                if (itemData == null) continue;

                int quantity = itemData.Id == "gold_coin"
                    ? _rng.Next(1, 6)
                    : 1;

                SpawnLootDrop(transform.Position, itemData, quantity);
            }
        }
    }

    private void SpawnLootDrop(Vector3 position, ItemData item, int quantity)
    {
        float height = _terrainGen.GetHeightAt(position.X, position.Z);
        float spawnY = height + 0.5f;

        // Offset slightly from death position
        float offsetX = (float)(_rng.NextDouble() * 2 - 1);
        float offsetZ = (float)(_rng.NextDouble() * 2 - 1);

        var lootEntity = _world.CreateEntity();
        _world.AddComponent(lootEntity, new TransformComponent
        {
            Position = new Vector3(position.X + offsetX, spawnY, position.Z + offsetZ),
            Scale = Vector3.One * 0.5f
        });
        _world.AddComponent(lootEntity, new LootDropComponent
        {
            Item = item,
            Quantity = quantity,
            SpawnY = spawnY
        });
        _world.AddComponent(lootEntity, new MeshRendererComponent
        {
            Mesh = _lootMesh,
            Shader = _basicShader
        });
    }

    private void TryPickupNearby(Entity playerEntity, Vector3 playerPos, InventoryComponent inventory)
    {
        var toPickup = new List<Entity>();

        foreach (var (entity, loot, transform) in _world.Query<LootDropComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist > loot.PickupRadius) continue;

            if (inventory.TryAdd(loot.Item, loot.Quantity))
                toPickup.Add(entity);
        }

        foreach (var entity in toPickup)
            _world.DestroyEntity(entity);
    }

    private void TryUseHealthPotion(Entity entity, InventoryComponent inventory)
    {
        var potionData = ItemDatabase.Get("health_potion");
        if (potionData == null) return;
        if (inventory.CountOf("health_potion") <= 0) return;

        if (!_world.HasComponent<HealthComponent>(entity)) return;
        var health = _world.GetComponent<HealthComponent>(entity);

        if (health.Current >= health.Max) return; // Already full

        health.Heal(potionData.HealthBonus);
        inventory.TryRemove("health_potion");
    }

    private void TryEquipAt(Entity entity, InventoryComponent inventory, int slotIndex)
    {
        var stack = inventory.GetAt(slotIndex);
        if (stack == null) return;
        if (stack.Item.Slot == ItemSlot.None) return; // Can't equip non-equipment

        if (!_world.HasComponent<EquipmentComponent>(entity)) return;
        var equipment = _world.GetComponent<EquipmentComponent>(entity);

        // Swap: unequip current, equip new
        var currentEquipped = equipment.GetSlot(stack.Item.Slot);
        if (currentEquipped != null)
        {
            inventory.TryAdd(currentEquipped.Item, currentEquipped.Quantity);
        }

        equipment.SetSlot(stack.Item.Slot, new ItemStack { Item = stack.Item, Quantity = 1 });
        inventory.TryRemove(stack.Item.Id);
    }

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryDeadEnemies()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (enemy.State != EnemyState.Dead) continue;
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _lootMesh.Dispose();
        _basicShader.Dispose();
    }
}
