using System.Numerics;
using CongCraft.Engine.ECS;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using CongCraft.Engine.Magic;
using CongCraft.Engine.Quest;
using CongCraft.Engine.SaveLoad;

namespace CongCraft.Engine.Tests.SaveLoad;

public class SaveLoadIntegrationTests
{
    private static CongCraft.Engine.Core.ServiceLocator CreateServices(World world)
    {
        var services = new CongCraft.Engine.Core.ServiceLocator();
        services.Register(world);
        return services;
    }

    private World CreateWorldWithPlayer()
    {
        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new PlayerComponent());
        world.AddComponent(player, new TransformComponent
        {
            Position = new Vector3(15f, 8f, -5f)
        });
        world.AddComponent(player, new HealthComponent { Current = 80f, Max = 100f });
        world.AddComponent(player, new ManaComponent { Current = 35f, Max = 50f });
        world.AddComponent(player, new LevelComponent { Level = 3, Experience = 150, SkillPoints = 2 });
        world.AddComponent(player, new SkillTree { Strength = 2, Endurance = 1, Agility = 3 });

        var inv = new InventoryComponent();
        inv.TryAdd(ItemDatabase.Get("iron_sword")!, 1);
        inv.TryAdd(ItemDatabase.Get("health_potion")!, 3);
        inv.TryAdd(ItemDatabase.Get("gold_coin")!, 10);
        world.AddComponent(player, inv);

        var equip = new EquipmentComponent();
        equip.MainHand = new ItemStack { Item = ItemDatabase.Get("iron_sword")!, Quantity = 1 };
        world.AddComponent(player, equip);

        var journal = new QuestJournal();
        world.AddComponent(player, journal);

        return world;
    }

    [Fact]
    public void ExtractAndApply_PreservesPlayerPosition()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();

        Assert.Equal(15f, save.Player.PositionX);
        Assert.Equal(8f, save.Player.PositionY);
        Assert.Equal(-5f, save.Player.PositionZ);
    }

    [Fact]
    public void ExtractAndApply_PreservesHealth()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal(80f, save.Player.Health);
        Assert.Equal(100f, save.Player.MaxHealth);
    }

    [Fact]
    public void ExtractAndApply_PreservesMana()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal(35f, save.Player.Mana);
        Assert.Equal(50f, save.Player.MaxMana);
    }

    [Fact]
    public void ExtractAndApply_PreservesLevel()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal(3, save.Player.Level);
        Assert.Equal(150, save.Player.Experience);
        Assert.Equal(2, save.Player.SkillPoints);
    }

    [Fact]
    public void ExtractAndApply_PreservesSkills()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal(2, save.Player.Strength);
        Assert.Equal(1, save.Player.Endurance);
        Assert.Equal(3, save.Player.Agility);
    }

    [Fact]
    public void ExtractAndApply_PreservesInventory()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal(3, save.Inventory.Count);
        Assert.Contains(save.Inventory, i => i.ItemId == "iron_sword" && i.Quantity == 1);
        Assert.Contains(save.Inventory, i => i.ItemId == "health_potion" && i.Quantity == 3);
        Assert.Contains(save.Inventory, i => i.ItemId == "gold_coin" && i.Quantity == 10);
    }

    [Fact]
    public void ExtractAndApply_PreservesEquipment()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        Assert.Equal("iron_sword", save.Equipment.MainHandId);
        Assert.Null(save.Equipment.HeadId);
    }

    [Fact]
    public void ApplySaveData_RestoresState()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        // Create a save with different values
        var save = new SaveData
        {
            Player = new PlayerSaveData
            {
                PositionX = 99f, PositionY = 22f, PositionZ = 33f,
                Health = 50f, MaxHealth = 150f,
                Mana = 10f, MaxMana = 80f,
                Level = 7, Experience = 500, SkillPoints = 5,
                Strength = 8, Endurance = 6, Agility = 4
            },
            Inventory =
            {
                new InventoryItemSave { ItemId = "dark_blade", Quantity = 1 }
            },
            Equipment = new EquipmentSaveData { MainHandId = "dark_blade" }
        };

        system.ApplySaveData(save);

        // Verify state was applied
        foreach (var (entity, player, transform) in world.Query<PlayerComponent, TransformComponent>())
        {
            Assert.Equal(99f, transform.Position.X);
            Assert.Equal(22f, transform.Position.Y);

            var health = world.GetComponent<HealthComponent>(entity);
            Assert.Equal(50f, health.Current);

            var mana = world.GetComponent<ManaComponent>(entity);
            Assert.Equal(10f, mana.Current);

            var level = world.GetComponent<LevelComponent>(entity);
            Assert.Equal(7, level.Level);

            var skills = world.GetComponent<SkillTree>(entity);
            Assert.Equal(8, skills.Strength);

            var inv = world.GetComponent<InventoryComponent>(entity);
            Assert.Single(inv.Items);
            Assert.Equal("dark_blade", inv.Items[0].Item.Id);

            var equip = world.GetComponent<EquipmentComponent>(entity);
            Assert.Equal("dark_blade", equip.MainHand?.Item.Id);
        }
    }

    [Fact]
    public void FullRoundTrip_SerializeAndDeserialize()
    {
        var world = CreateWorldWithPlayer();
        var system = new SaveLoadSystem();
        system.Initialize(CreateServices(world));

        var save = system.ExtractSaveData();
        string json = SaveSerializer.SerializeToString(save);
        var loaded = SaveSerializer.DeserializeFromString(json)!;

        Assert.Equal(save.Player.PositionX, loaded.Player.PositionX);
        Assert.Equal(save.Player.Health, loaded.Player.Health);
        Assert.Equal(save.Player.Level, loaded.Player.Level);
        Assert.Equal(save.Inventory.Count, loaded.Inventory.Count);
    }
}
