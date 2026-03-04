using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Magic;

/// <summary>
/// Tracks active spell effects and cooldowns on an entity.
/// </summary>
public sealed class SpellState : IComponent
{
    public int SelectedSpell { get; set; } // 0-3 index into spell bar
    public float[] Cooldowns { get; } = new float[4]; // Per spell cooldown timers
    public float ShieldTimer { get; set; } // Remaining shield duration
    public float ShieldDamageReduction { get; set; } = 0.5f;
    public bool HasActiveShield => ShieldTimer > 0;
}
