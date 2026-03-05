using CongCraft.Engine.Core;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.ECS.Systems;

/// <summary>
/// Manages an ordered list of systems. Runs update and render passes each frame.
/// Failed systems are skipped instead of crashing the game.
/// </summary>
public sealed class SystemManager : IDisposable
{
    private readonly List<ISystem> _systems = new();
    private readonly HashSet<ISystem> _failedSystems = new();
    private bool _initialized;

    public void Register(ISystem system)
    {
        _systems.Add(system);
        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void InitializeAll(ServiceLocator services)
    {
        foreach (var system in _systems)
        {
            var name = system.GetType().Name;
            try
            {
                DevLog.Info($"  Init: {name} (priority {system.Priority})");
                system.Initialize(services);
            }
            catch (Exception ex)
            {
                DevLog.Error($"  FAILED: {name}", ex);
                _failedSystems.Add(system);
            }
        }

        if (_failedSystems.Count > 0)
            DevLog.Warn($"{_failedSystems.Count} system(s) failed to initialize and will be skipped");

        _initialized = true;
    }

    public void UpdateAll(GameTime time)
    {
        if (!_initialized) return;
        foreach (var system in _systems)
        {
            if (_failedSystems.Contains(system)) continue;
            system.Update(time);
        }
    }

    public void RenderAll(GameTime time)
    {
        if (!_initialized) return;
        foreach (var system in _systems)
        {
            if (_failedSystems.Contains(system)) continue;
            system.Render(time);
        }
    }

    /// <summary>
    /// Render shadow-casting geometry into the shadow map.
    /// Systems that implement IShadowCaster will be called.
    /// </summary>
    public void RenderShadowPass(ShadowMap shadowMap)
    {
        if (!_initialized) return;
        foreach (var system in _systems)
        {
            if (_failedSystems.Contains(system)) continue;
            if (system is IShadowCaster caster)
                caster.RenderShadowPass(shadowMap);
        }
    }

    public IReadOnlyList<ISystem> Systems => _systems;

    public void Dispose()
    {
        foreach (var system in _systems)
        {
            if (_failedSystems.Contains(system)) continue;
            try
            {
                system.Dispose();
            }
            catch (Exception ex)
            {
                DevLog.Error($"Error disposing {system.GetType().Name}", ex);
            }
        }
        _systems.Clear();
    }
}
