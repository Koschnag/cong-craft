using CongCraft.Engine.Core;

namespace CongCraft.Engine.ECS.Systems;

/// <summary>
/// All game systems implement this interface.
/// Systems process entities with specific components each frame.
/// </summary>
public interface ISystem : IDisposable
{
    /// <summary>
    /// Lower priority runs first.
    /// </summary>
    int Priority { get; }

    void Initialize(ServiceLocator services);
    void Update(GameTime time);
    void Render(GameTime time);
}
