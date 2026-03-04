using System.Numerics;

namespace CongCraft.Engine.UI;

/// <summary>
/// Calculates health bar display rectangles.
/// </summary>
public static class HealthBar
{
    public static (HudElement Background, HudElement Fill) Calculate(
        float health, float maxHealth, Vector2 position, Vector2 size)
    {
        var background = new HudElement(position, size, new Vector4(0.2f, 0.05f, 0.05f, 0.8f));
        float fillWidth = size.X * (health / maxHealth);
        var fill = new HudElement(position, new Vector2(fillWidth, size.Y), new Vector4(0.7f, 0.1f, 0.1f, 0.9f));
        return (background, fill);
    }
}
