using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;

namespace CongCraft.Engine.Inventory;

/// <summary>
/// Applies equipment stat bonuses to the player's combat component each frame.
/// </summary>
public sealed class EquipmentSystem : ISystem
{
    public int Priority => 12; // After player setup, before combat

    private World _world = null!;

    // Store base stats to avoid stacking bonuses each frame
    private float _baseDamage;
    private float _baseBlockReduction;
    private float _baseMoveSpeed;
    private float _baseRunSpeed;
    private bool _basesCaptured;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
    }

    public void Update(GameTime time)
    {
        foreach (var (entity, player, combat) in _world.Query<PlayerComponent, CombatComponent>())
        {
            if (!_world.HasComponent<EquipmentComponent>(entity)) continue;
            var equipment = _world.GetComponent<EquipmentComponent>(entity);

            // Capture base stats once
            if (!_basesCaptured)
            {
                _baseDamage = combat.AttackDamage;
                _baseBlockReduction = combat.BlockDamageReduction;
                _baseMoveSpeed = player.MoveSpeed;
                _baseRunSpeed = player.RunSpeed;
                _basesCaptured = true;
            }

            // Apply equipment bonuses
            combat.AttackDamage = _baseDamage + equipment.TotalAttackBonus;

            // Defense bonus reduces block reduction needed (better block)
            float defensePercent = MathF.Min(0.3f, equipment.TotalDefenseBonus * 0.02f);
            combat.BlockDamageReduction = MathF.Min(0.95f, _baseBlockReduction + defensePercent);

            // Speed bonus modifies move/run speed
            player.MoveSpeed = MathF.Max(1f, _baseMoveSpeed + equipment.TotalSpeedBonus);
            player.RunSpeed = MathF.Max(2f, _baseRunSpeed + equipment.TotalSpeedBonus * 1.5f);
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
