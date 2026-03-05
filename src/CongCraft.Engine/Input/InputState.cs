using System.Numerics;
using CongCraft.Engine.ECS;
using Silk.NET.Input;

namespace CongCraft.Engine.Input;

/// <summary>
/// Snapshot of input state for the current frame. Stored as a singleton component.
/// </summary>
public sealed class InputState : IComponent
{
    public HashSet<Key> KeysDown { get; } = new();
    public HashSet<Key> KeysPressed { get; } = new();
    public HashSet<Key> KeysReleased { get; } = new();
    public HashSet<MouseButton> MouseButtonsDown { get; } = new();
    public HashSet<MouseButton> MouseButtonsPressed { get; } = new();
    public HashSet<MouseButton> MouseButtonsReleased { get; } = new();
    public Vector2 MousePosition { get; set; }
    public Vector2 MouseDelta { get; set; }
    public float ScrollDelta { get; set; }
    public bool IsMouseCaptured { get; set; }

    public bool IsKeyDown(Key key) => KeysDown.Contains(key);
    public bool IsKeyPressed(Key key) => KeysPressed.Contains(key);
    public bool IsMouseButtonDown(MouseButton button) => MouseButtonsDown.Contains(button);
    public bool IsMouseButtonPressed(MouseButton button) => MouseButtonsPressed.Contains(button);

    public void BeginFrame()
    {
        KeysPressed.Clear();
        KeysReleased.Clear();
        MouseButtonsPressed.Clear();
        MouseButtonsReleased.Clear();
        MouseDelta = Vector2.Zero;
        ScrollDelta = 0;
    }
}
