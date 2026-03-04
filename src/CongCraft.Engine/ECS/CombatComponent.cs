namespace CongCraft.Engine.ECS;

/// <summary>
/// Tracks combat state for any entity that can fight (player or enemy).
/// </summary>
public sealed class CombatComponent : IComponent
{
    public float AttackDamage { get; set; } = 10f;
    public float AttackRange { get; set; } = 2.5f;
    public float AttackCooldown { get; set; } = 0.8f;
    public float BlockDamageReduction { get; set; } = 0.7f;
    public float DodgeDistance { get; set; } = 3f;
    public float DodgeCooldown { get; set; } = 1.0f;
    public float DodgeDuration { get; set; } = 0.3f;

    // Runtime state
    public float AttackTimer { get; set; }
    public float DodgeTimer { get; set; }
    public float DodgeCooldownTimer { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsBlocking { get; set; }
    public bool IsDodging { get; set; }
    public float AttackAnimationProgress { get; set; } // 0..1
}
