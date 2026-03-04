using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Magic;

/// <summary>
/// Tracks mana pool for spell casting.
/// </summary>
public sealed class ManaComponent : IComponent
{
    public float Current { get; set; } = 50f;
    public float Max { get; set; } = 50f;
    public float RegenRate { get; set; } = 2f; // Mana per second
    public float Percentage => Max > 0 ? Current / Max : 0;

    public bool CanSpend(float amount) => Current >= amount;

    public bool TrySpend(float amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }

    public void Regenerate(float deltaTime)
    {
        Current = MathF.Min(Max, Current + RegenRate * deltaTime);
    }
}
