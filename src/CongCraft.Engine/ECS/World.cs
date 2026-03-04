namespace CongCraft.Engine.ECS;

/// <summary>
/// Central entity-component store. Creates entities, attaches components, and queries.
/// Simple dictionary-based storage — sufficient for hundreds of entities.
/// </summary>
public sealed class World
{
    private int _nextId;
    private readonly Dictionary<int, Dictionary<Type, IComponent>> _entities = new();
    private readonly Dictionary<Type, IComponent> _singletons = new();

    public Entity CreateEntity()
    {
        var entity = new Entity(_nextId++);
        _entities[entity.Id] = new Dictionary<Type, IComponent>();
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        _entities.Remove(entity.Id);
    }

    public void AddComponent<T>(Entity entity, T component) where T : IComponent
    {
        if (!_entities.TryGetValue(entity.Id, out var components))
            throw new InvalidOperationException($"Entity {entity.Id} does not exist.");

        components[typeof(T)] = component;
    }

    public T GetComponent<T>(Entity entity) where T : IComponent
    {
        if (!_entities.TryGetValue(entity.Id, out var components))
            throw new InvalidOperationException($"Entity {entity.Id} does not exist.");

        if (!components.TryGetValue(typeof(T), out var component))
            throw new InvalidOperationException($"Entity {entity.Id} has no {typeof(T).Name}.");

        return (T)component;
    }

    public bool HasComponent<T>(Entity entity) where T : IComponent
    {
        return _entities.TryGetValue(entity.Id, out var components)
               && components.ContainsKey(typeof(T));
    }

    public void SetComponent<T>(Entity entity, T component) where T : IComponent
    {
        AddComponent(entity, component);
    }

    /// <summary>
    /// Query all entities that have a specific component type.
    /// </summary>
    public IEnumerable<(Entity Entity, T Component)> Query<T>() where T : IComponent
    {
        foreach (var (id, components) in _entities)
        {
            if (components.TryGetValue(typeof(T), out var component))
                yield return (new Entity(id), (T)component);
        }
    }

    /// <summary>
    /// Query all entities that have two specific component types.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 C1, T2 C2)> Query<T1, T2>()
        where T1 : IComponent where T2 : IComponent
    {
        foreach (var (id, components) in _entities)
        {
            if (components.TryGetValue(typeof(T1), out var c1)
                && components.TryGetValue(typeof(T2), out var c2))
                yield return (new Entity(id), (T1)c1, (T2)c2);
        }
    }

    /// <summary>
    /// Store a singleton component (global state like InputState, Camera data).
    /// </summary>
    public void SetSingleton<T>(T component) where T : IComponent
    {
        _singletons[typeof(T)] = component;
    }

    public T GetSingleton<T>() where T : IComponent
    {
        if (_singletons.TryGetValue(typeof(T), out var component))
            return (T)component;

        throw new InvalidOperationException($"Singleton {typeof(T).Name} not registered.");
    }

    public bool TryGetSingleton<T>(out T? component) where T : class, IComponent
    {
        if (_singletons.TryGetValue(typeof(T), out var c))
        {
            component = (T)c;
            return true;
        }
        component = null;
        return false;
    }

    public int EntityCount => _entities.Count;
}
