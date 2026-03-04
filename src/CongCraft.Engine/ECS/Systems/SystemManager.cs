using CongCraft.Engine.Core;

namespace CongCraft.Engine.ECS.Systems;

/// <summary>
/// Manages an ordered list of systems. Runs update and render passes each frame.
/// </summary>
public sealed class SystemManager : IDisposable
{
    private readonly List<ISystem> _systems = new();
    private bool _initialized;

    public void Register(ISystem system)
    {
        _systems.Add(system);
        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void InitializeAll(ServiceLocator services)
    {
        foreach (var system in _systems)
            system.Initialize(services);
        _initialized = true;
    }

    public void UpdateAll(GameTime time)
    {
        if (!_initialized) return;
        foreach (var system in _systems)
            system.Update(time);
    }

    public void RenderAll(GameTime time)
    {
        if (!_initialized) return;
        foreach (var system in _systems)
            system.Render(time);
    }

    public IReadOnlyList<ISystem> Systems => _systems;

    public void Dispose()
    {
        foreach (var system in _systems)
            system.Dispose();
        _systems.Clear();
    }
}
