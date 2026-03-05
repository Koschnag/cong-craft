using System.Numerics;

namespace CongCraft.Engine.ECS;

/// <summary>
/// AI state for enemies. Controls patrol, chase, and attack behavior.
/// </summary>
public sealed class EnemyComponent : IComponent
{
    public EnemyType Type { get; set; } = EnemyType.Wolf;
    public EnemyState State { get; set; } = EnemyState.Patrol;
    public float DetectionRange { get; set; } = 15f;
    public float AttackRange { get; set; } = 2.5f;
    public float MoveSpeed { get; set; } = 2.5f;
    public float ChaseSpeed { get; set; } = 4f;
    public float PatrolRadius { get; set; } = 10f;

    // Patrol state
    public Vector3 SpawnPosition { get; set; }
    public Vector3 PatrolTarget { get; set; }
    public float PatrolWaitTimer { get; set; }
    public float PatrolWaitDuration { get; set; } = 2f;

    // Attack state
    public float AttackTimer { get; set; }
    public float AttackCooldown { get; set; } = 1.2f;

    // Death
    public bool IsDead { get; set; }
    public float DeathTimer { get; set; }
}

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Dead
}

/// <summary>
/// Different enemy archetypes with distinct stats and behaviors.
/// </summary>
public enum EnemyType
{
    Wolf,     // Fast, low HP, pack hunter
    Bandit,   // Balanced, medium stats
    Skeleton, // Slow, high damage, medium HP
    Troll     // Very slow, very high HP and damage (mini-boss)
}
