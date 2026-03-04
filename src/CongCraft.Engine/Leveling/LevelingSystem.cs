using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using Silk.NET.Input;

namespace CongCraft.Engine.Leveling;

/// <summary>
/// Handles XP gain from enemy kills, level-ups, and skill point allocation.
/// Press L to open skill menu, 1/2/3 to allocate points.
/// </summary>
public sealed class LevelingSystem : ISystem
{
    public int Priority => 28; // After combat and quest

    private World _world = null!;

    // Track which enemies already gave XP (by entity ID)
    private readonly HashSet<int> _xpAwardedEnemies = new();

    // Base XP for killing an enemy
    private const int BaseEnemyXp = 25;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();

        // Award XP for killed enemies
        AwardKillXp();

        // Handle skill allocation input
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (!_world.HasComponent<LevelComponent>(entity)) continue;
            if (!_world.HasComponent<SkillTree>(entity)) continue;

            var level = _world.GetComponent<LevelComponent>(entity);
            var skills = _world.GetComponent<SkillTree>(entity);

            // Toggle skill menu with L
            if (input.IsKeyPressed(Key.L))
                level.IsSkillMenuOpen = !level.IsSkillMenuOpen;

            // Allocate points when menu is open
            if (level.IsSkillMenuOpen && level.SkillPoints > 0)
            {
                if (input.IsKeyPressed(Key.Number1))
                    skills.TryAllocate(SkillType.Strength, level);
                if (input.IsKeyPressed(Key.Number2))
                    skills.TryAllocate(SkillType.Endurance, level);
                if (input.IsKeyPressed(Key.Number3))
                    skills.TryAllocate(SkillType.Agility, level);
            }

            // Apply skill bonuses to stats each frame
            ApplySkillBonuses(entity, skills);
        }
    }

    private void AwardKillXp()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (enemy.State != EnemyState.Dead) continue;
            if (_xpAwardedEnemies.Contains(entity.Id)) continue;

            _xpAwardedEnemies.Add(entity.Id);

            // Find player and award XP
            foreach (var (playerEntity, player) in _world.Query<PlayerComponent>())
            {
                if (!_world.HasComponent<LevelComponent>(playerEntity)) continue;
                var level = _world.GetComponent<LevelComponent>(playerEntity);

                // Stronger enemies give more XP
                int xp = BaseEnemyXp + (int)(health.Max * 0.2f);
                level.AddExperience(xp);
            }
        }
    }

    private void ApplySkillBonuses(Entity entity, SkillTree skills)
    {
        // Apply to combat stats
        if (_world.HasComponent<CombatComponent>(entity))
        {
            var combat = _world.GetComponent<CombatComponent>(entity);
            // Base damage (15) + equipment bonus is handled by EquipmentSystem
            // We add skill bonus on top
            combat.SkillAttackBonus = skills.TotalAttackBonus;
            combat.SkillCooldownReduction = skills.TotalCooldownReduction;
        }

        // Apply to health
        if (_world.HasComponent<HealthComponent>(entity))
        {
            var health = _world.GetComponent<HealthComponent>(entity);
            health.SkillMaxHealthBonus = skills.TotalHealthBonus;
        }

        // Apply to movement speed
        if (_world.HasComponent<PlayerComponent>(entity))
        {
            var player = _world.GetComponent<PlayerComponent>(entity);
            player.SkillSpeedBonus = skills.TotalSpeedBonus;
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
