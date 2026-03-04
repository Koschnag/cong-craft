using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Renders all entities with MeshRendererComponent that are NOT handled by specialized systems.
/// Currently renders the player capsule.
/// </summary>
public sealed class RenderSystem : ISystem
{
    public int Priority => 100;

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _basicShader = null!;

    public Shader BasicShader => _basicShader;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        _basicShader.Use();
        _basicShader.SetUniform("uView", _camera.ViewMatrix);
        _basicShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _basicShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_basicShader);

        // Render entities that have a PlayerTag (player capsule)
        foreach (var (entity, meshComp) in _world.Query<MeshRendererComponent>())
        {
            if (!_world.HasComponent<PlayerComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);
            _basicShader.SetUniform("uModel", transform.ModelMatrix);
            meshComp.Mesh.Draw();
        }
    }

    public void Dispose()
    {
        _basicShader.Dispose();
    }
}
