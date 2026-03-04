using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace CongCraft.Engine.Core;

/// <summary>
/// Top-level engine coordinator. Owns the window, GL context, game loop, and system lifecycle.
/// </summary>
public sealed class GameEngine : IDisposable
{
    private IWindow _window = null!;
    private GL _gl = null!;
    private readonly SystemManager _systems = new();
    private readonly World _world = new();
    private readonly ServiceLocator _services = new();
    private readonly Camera _camera = new();
    private readonly LightingData _lighting = new();
    private GameTime _gameTime;
    private InputSystem? _inputSystem;
    private long _frameCount;

    public World World => _world;
    public ServiceLocator Services => _services;
    public Camera Camera => _camera;
    public LightingData Lighting => _lighting;

    public void RegisterSystem(ISystem system)
    {
        _systems.Register(system);
        if (system is InputSystem inputSys)
            _inputSystem = inputSys;
    }

    public void Run(string title = "CongCraft", int width = 1280, int height = 720)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(width, height),
            Title = title,
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core,
                ContextFlags.ForwardCompatible, new APIVersion(3, 3)),
            VSync = true
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClosing;
        _window.Run();
        _window.Dispose();
    }

    private void OnLoad()
    {
        _gl = GL.GetApi(_window);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);

        // Register core services
        _services.Register(_gl);
        _services.Register(_world);
        _services.Register(_window);
        _services.Register(_camera);
        _services.Register(_lighting);

        // Initialize all registered systems
        _systems.InitializeAll(_services);
    }

    private void OnUpdate(double deltaTime)
    {
        _frameCount++;
        _gameTime = new GameTime(deltaTime, _gameTime.TotalTime + deltaTime, _frameCount);

        _inputSystem?.BeginFrame();
        _systems.UpdateAll(_gameTime);
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _systems.RenderAll(_gameTime);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.AspectRatio = (float)size.X / size.Y;
    }

    private void OnClosing()
    {
        _systems.Dispose();
    }

    public void Dispose()
    {
        _systems.Dispose();
    }
}
