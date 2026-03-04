namespace CongCraft.Engine.ECS;

public sealed class PlayerComponent : IComponent
{
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float MoveSpeed { get; set; } = 5f;
    public float RunSpeed { get; set; } = 10f;
    public bool IsRunning { get; set; }
}
