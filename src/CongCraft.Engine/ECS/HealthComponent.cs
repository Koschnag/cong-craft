namespace CongCraft.Engine.ECS;

/// <summary>
/// Shared health component for any entity that can take damage.
/// Replaces the health fields in PlayerComponent for combat purposes.
/// </summary>
public sealed class HealthComponent : IComponent
{
    public float Current { get; set; } = 100f;
    public float Max { get; set; } = 100f;
    public bool IsAlive => Current > 0;
    public float Percentage => Max > 0 ? Current / Max : 0;

    public float DamageFlashTimer { get; set; }
    public float LastDamageAmount { get; set; }

    public void TakeDamage(float amount)
    {
        LastDamageAmount = amount;
        DamageFlashTimer = 0.3f;
        Current = MathF.Max(0, Current - amount);
    }

    public void Heal(float amount)
    {
        Current = MathF.Min(Max, Current + amount);
    }
}
