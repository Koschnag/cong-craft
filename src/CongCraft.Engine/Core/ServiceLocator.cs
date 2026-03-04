namespace CongCraft.Engine.Core;

/// <summary>
/// Minimal typed service locator. Stores shared services (GL context, Camera, etc.)
/// accessible by type. Not a full DI container — just a typed dictionary.
/// </summary>
public sealed class ServiceLocator
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
    }

    public bool TryGet<T>(out T? service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var s))
        {
            service = (T)s;
            return true;
        }
        service = null;
        return false;
    }
}
