namespace CongCraft.Engine.Magic;

/// <summary>
/// Defines a spell with its costs, effects, and cooldown.
/// </summary>
public sealed class SpellData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required SpellType Type { get; init; }
    public float ManaCost { get; init; }
    public float Cooldown { get; init; } = 1f;
    public float Damage { get; init; }
    public float HealAmount { get; init; }
    public float Duration { get; init; }
    public float Range { get; init; } = 10f;

    // VFX colors
    public float VfxR { get; init; } = 1f;
    public float VfxG { get; init; } = 1f;
    public float VfxB { get; init; } = 1f;
}

public enum SpellType
{
    Projectile,  // Fireball - damages enemies in range
    SelfHeal,    // Heal - restores player health
    Shield,      // Shield - temporary damage reduction
    AreaEffect   // Ice Nova - damages nearby enemies
}
