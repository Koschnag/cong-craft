using System.Numerics;

namespace CongCraft.Engine.Boss;

/// <summary>
/// Static definitions for boss encounters.
/// </summary>
public sealed class BossData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public float Health { get; init; }
    public float MeleeDamage { get; init; }
    public float SlamDamage { get; init; }
    public float SlamRadius { get; init; }
    public float ChargeDamage { get; init; }
    public float ChargeSpeed { get; init; }
    public float MeleeRange { get; init; }
    public float MeleeCooldown { get; init; }
    public float SpecialCooldown { get; init; }
    public int MaxPhases { get; init; }
    public float[] PhaseThresholds { get; init; } = Array.Empty<float>();
    public Vector3 SpawnPosition { get; init; }
    public float Scale { get; init; } = 2f;

    // Visual
    public float ColorR { get; init; }
    public float ColorG { get; init; }
    public float ColorB { get; init; }
}

public static class BossDatabase
{
    private static readonly Dictionary<string, BossData> Bosses = new();

    static BossDatabase()
    {
        Register(new BossData
        {
            Id = "forest_guardian",
            Name = "Forest Guardian",
            Health = 500f,
            MeleeDamage = 25f,
            SlamDamage = 35f,
            SlamRadius = 6f,
            ChargeDamage = 30f,
            ChargeSpeed = 12f,
            MeleeRange = 3.5f,
            MeleeCooldown = 1.2f,
            SpecialCooldown = 5f,
            MaxPhases = 3,
            PhaseThresholds = new[] { 0.66f, 0.33f },
            SpawnPosition = new Vector3(-30f, 0, -30f),
            Scale = 2.5f,
            ColorR = 0.3f,
            ColorG = 0.5f,
            ColorB = 0.2f
        });

        Register(new BossData
        {
            Id = "shadow_knight",
            Name = "Shadow Knight",
            Health = 400f,
            MeleeDamage = 30f,
            SlamDamage = 20f,
            SlamRadius = 4f,
            ChargeDamage = 40f,
            ChargeSpeed = 15f,
            MeleeRange = 3f,
            MeleeCooldown = 0.8f,
            SpecialCooldown = 4f,
            MaxPhases = 2,
            PhaseThresholds = new[] { 0.5f },
            SpawnPosition = new Vector3(40f, 0, -25f),
            Scale = 2f,
            ColorR = 0.15f,
            ColorG = 0.1f,
            ColorB = 0.25f
        });
    }

    private static void Register(BossData boss) => Bosses[boss.Id] = boss;

    public static BossData? Get(string id) => Bosses.GetValueOrDefault(id);

    public static IReadOnlyDictionary<string, BossData> All => Bosses;
}
