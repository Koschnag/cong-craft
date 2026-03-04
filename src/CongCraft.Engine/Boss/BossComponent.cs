using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Boss;

/// <summary>
/// Marks an entity as a boss with phases and special attacks.
/// </summary>
public sealed class BossComponent : IComponent
{
    public required string BossId { get; init; }
    public required string Name { get; init; }
    public int CurrentPhase { get; set; }
    public int MaxPhases { get; init; } = 3;
    public BossState State { get; set; } = BossState.Idle;
    public bool IsActivated { get; set; }
    public float ActivationRadius { get; init; } = 15f;
    public float ArenaRadius { get; init; } = 20f;

    // Attack state
    public float AttackTimer { get; set; }
    public float SpecialAttackTimer { get; set; }
    public BossAttackType CurrentAttack { get; set; } = BossAttackType.None;
    public float AttackProgress { get; set; } // 0..1

    // Phase transition health thresholds (as percentage)
    public float[] PhaseThresholds { get; init; } = { 0.66f, 0.33f };
}

public enum BossState
{
    Idle,
    Activated,
    Attacking,
    SpecialAttack,
    PhaseTransition,
    Enraged,
    Dead
}

public enum BossAttackType
{
    None,
    Melee,          // Basic melee swing (higher damage than normal enemies)
    Slam,           // Ground slam - AOE damage around boss
    Charge,         // Rush toward player
    Summon          // Summon minion enemies
}
