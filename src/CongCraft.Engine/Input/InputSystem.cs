using System.Numerics;
using System.Runtime.InteropServices;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace CongCraft.Engine.Input;

/// <summary>
/// Polls Silk.NET input devices and updates the InputState singleton each frame.
/// </summary>
public sealed class InputSystem : ISystem
{
    public int Priority => 0;

    private World _world = null!;
    private IInputContext? _input;
    private InputState _state = null!;
    private Vector2 _lastMousePos;
    private bool _firstMouse = true;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        var window = services.Get<IWindow>();
        _input = window.CreateInput();
        _state = new InputState();
        _world.SetSingleton(_state);

        foreach (var keyboard in _input.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        foreach (var mouse in _input.Mice)
        {
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnScroll;

            // CursorMode.Raw can crash on some macOS versions — fall back to Disabled
            try
            {
                mouse.Cursor.CursorMode = CursorMode.Raw;
            }
            catch
            {
                DevLog.Warn("CursorMode.Raw not supported, falling back to Disabled");
                mouse.Cursor.CursorMode = CursorMode.Disabled;
            }
        }

        _state.IsMouseCaptured = true;
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        if (!_state.KeysDown.Contains(key))
            _state.KeysPressed.Add(key);
        _state.KeysDown.Add(key);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        _state.KeysDown.Remove(key);
        _state.KeysReleased.Add(key);
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (_firstMouse)
        {
            _lastMousePos = position;
            _firstMouse = false;
            return;
        }

        _state.MouseDelta += position - _lastMousePos;
        _state.MousePosition = position;
        _lastMousePos = position;
    }

    private void OnScroll(IMouse mouse, ScrollWheel scroll)
    {
        _state.ScrollDelta += scroll.Y;
    }

    public void Update(GameTime time)
    {
        // InputState is updated via callbacks, we just need to clear per-frame data at start
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _input?.Dispose();
    }

    /// <summary>
    /// Called at the beginning of each frame to reset per-frame input state.
    /// </summary>
    public void BeginFrame()
    {
        _state.BeginFrame();
    }
}
