using CongCraft.Engine.ECS;

namespace CongCraft.Engine.SaveLoad;

/// <summary>
/// Singleton for displaying save/load notifications on the HUD.
/// </summary>
public sealed class SaveNotification : IComponent
{
    public string Text { get; set; } = "";
    public float Timer { get; set; }
}
