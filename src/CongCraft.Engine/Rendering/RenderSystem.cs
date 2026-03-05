using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Renders the player entity (MeshRendererComponent + PlayerComponent).
/// Enemies are handled by EnemyRenderSystem. Includes shadow map support.
/// </summary>
public sealed class RenderSystem : ISystem, IShadowCaster
{
    public int Priority => 100;

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _basicShader = null!;
    private ShadowMap? _shadowMap;

    public Shader BasicShader => _basicShader;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _shadowMap = services.Get<ShadowMap>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time) { }

    public void RenderShadowPass(ShadowMap shadowMap)
    {
        var shader = shadowMap.GetEntityShader();
        foreach (var (entity, meshComp) in _world.Query<MeshRendererComponent>())
        {
            if (!_world.HasComponent<PlayerComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);
            shader.SetUniform("uModel", transform.ModelMatrix);
            meshComp.Mesh.Draw();
        }
    }

    public void Render(GameTime time)
    {
        _basicShader.Use();
        _basicShader.SetUniform("uView", _camera.ViewMatrix);
        _basicShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _basicShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_basicShader);
        _shadowMap?.BindToShader(_basicShader, 0);

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
