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
    private IWindow _window = null!;
    private IInputContext? _input;
    private InputState _state = null!;
    private Vector2 _lastMousePos;
    private bool _firstMouse = true;
    private bool _lastCapturedState;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _window = services.Get<IWindow>();
        _input = _window.CreateInput();
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
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
        }

        // Start with mouse free — MenuSystem will capture it when entering gameplay
        _state.IsMouseCaptured = false;
        SetCursorMode(false);
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

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (!_state.MouseButtonsDown.Contains(button))
            _state.MouseButtonsPressed.Add(button);
        _state.MouseButtonsDown.Add(button);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        _state.MouseButtonsDown.Remove(button);
        _state.MouseButtonsReleased.Add(button);
    }

    private void OnScroll(IMouse mouse, ScrollWheel scroll)
    {
        _state.ScrollDelta += scroll.Y;
    }

    private void SetCursorMode(bool captured)
    {
        if (_input == null) return;
        foreach (var mouse in _input.Mice)
        {
            if (captured)
            {
                try
                {
                    mouse.Cursor.CursorMode = CursorMode.Raw;
                }
                catch
                {
                    mouse.Cursor.CursorMode = CursorMode.Disabled;
                }
            }
            else
            {
                mouse.Cursor.CursorMode = CursorMode.Normal;
            }
        }
    }

    public void Update(GameTime time)
    {
        // Sync cursor mode with IsMouseCaptured state
        if (_state.IsMouseCaptured != _lastCapturedState)
        {
            SetCursorMode(_state.IsMouseCaptured);
            _lastCapturedState = _state.IsMouseCaptured;
            _firstMouse = true; // Reset to avoid delta jump
        }
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
        _state?.BeginFrame();
    }
}
