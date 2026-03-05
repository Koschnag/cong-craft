using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Input.Glfw;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;

namespace CongCraft.Engine.Core;

/// <summary>
/// Top-level engine coordinator. Owns the window, GL context, game loop, and system lifecycle.
/// Now includes shadow mapping and post-processing pipeline.
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

    private ShadowMap? _shadowMap;
    private PostProcessing? _postProcessing;

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

    public void Run(string title = "CongCraft", int width = 1920, int height = 1080)
    {
        DevLog.Section("Window Creation");
        DevLog.Info($"Creating window: {title} ({width}x{height})");

        // Explicitly register GLFW platforms for Native AOT compatibility
        GlfwWindowing.RegisterPlatform();
        GlfwInput.RegisterPlatform();

        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(width, height),
            Title = title,
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core,
                ContextFlags.ForwardCompatible, new APIVersion(3, 3)),
            VSync = true
        };

        try
        {
            _window = Window.Create(options);
            DevLog.Info("Window created successfully");
        }
        catch (Exception ex)
        {
            DevLog.Error("Failed to create window", ex);
            throw;
        }

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClosing;

        DevLog.Info("Starting window run loop...");
        _window.Run();
        _window.Dispose();
    }

    private void OnLoad()
    {
        DevLog.Section("OpenGL Init");

        try
        {
            _gl = GL.GetApi(_window);
            DevLog.Info($"OpenGL Vendor:   {_gl.GetStringS(StringName.Vendor)}");
            DevLog.Info($"OpenGL Renderer: {_gl.GetStringS(StringName.Renderer)}");
            DevLog.Info($"OpenGL Version:  {_gl.GetStringS(StringName.Version)}");
            DevLog.Info($"GLSL Version:    {_gl.GetStringS(StringName.ShadingLanguageVersion)}");
        }
        catch (Exception ex)
        {
            DevLog.Error("Failed to get OpenGL API", ex);
            throw;
        }

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

        // Initialize shadow map and post-processing
        _shadowMap = new ShadowMap(_gl);
        _services.Register(_shadowMap);

        _postProcessing = new PostProcessing(_gl, _window.Size.X, _window.Size.Y);
        _services.Register(_postProcessing);

        DevLog.Section("System Init");
        _systems.InitializeAll(_services);

        DevLog.Info("All systems initialized — entering game loop");
    }

    private void OnUpdate(double deltaTime)
    {
        _frameCount++;
        _gameTime = new GameTime(deltaTime, _gameTime.TotalTime + deltaTime, _frameCount);

        _systems.UpdateAll(_gameTime);

        // Clear per-frame input AFTER all systems have read it.
        // Silk.NET fires input callbacks during DoEvents() before OnUpdate,
        // so we must let systems read the current frame's input first.
        _inputSystem?.BeginFrame();
    }

    private void OnRender(double deltaTime)
    {
        if (_shadowMap == null || _postProcessing == null) return;

        // Update shadow map light matrix
        _shadowMap.UpdateLightMatrix(_lighting.SunDirection, _camera.Target);

        // --- Shadow pass ---
        _shadowMap.BeginPass();
        _systems.RenderShadowPass(_shadowMap);
        _shadowMap.EndPass(_window.Size.X, _window.Size.Y);

        // --- Scene pass (into HDR FBO) ---
        _postProcessing.BeginScene();
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _systems.RenderAll(_gameTime);
        _postProcessing.EndSceneAndApply();
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.AspectRatio = (float)size.X / size.Y;
        _postProcessing?.Resize(size.X, size.Y);
    }

    private void OnClosing()
    {
        DevLog.Section("Shutdown");
        _systems.Dispose();
        _shadowMap?.Dispose();
        _postProcessing?.Dispose();
    }

    public void Dispose()
    {
        _systems.Dispose();
        _shadowMap?.Dispose();
        _postProcessing?.Dispose();
    }
}
