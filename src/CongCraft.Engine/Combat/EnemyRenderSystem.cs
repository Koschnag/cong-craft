using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Renders enemies with shadow support and point lights.
/// </summary>
public sealed class EnemyRenderSystem : ISystem, IShadowCaster
{
    public int Priority => 101; // After main render

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _basicShader = null!;
    private ShadowMap? _shadowMap;

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
        foreach (var (entity, enemy, meshComp) in _world.Query<EnemyComponent, MeshRendererComponent>())
        {
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);
            var model = transform.ModelMatrix;
            if (enemy.State == EnemyState.Dead)
            {
                float fadeProgress = 1f - (enemy.DeathTimer / 3f);
                model = System.Numerics.Matrix4x4.CreateScale(1f - fadeProgress) * model;
            }
            shader.SetUniform("uModel", model);
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

        foreach (var (entity, enemy, meshComp) in _world.Query<EnemyComponent, MeshRendererComponent>())
        {
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);

            var model = transform.ModelMatrix;
            if (enemy.State == EnemyState.Dead)
            {
                float fadeProgress = 1f - (enemy.DeathTimer / 3f);
                float scale = 1f - fadeProgress;
                model = System.Numerics.Matrix4x4.CreateScale(scale) * model;
            }

            _basicShader.SetUniform("uModel", model);
            meshComp.Mesh.Draw();
        }
    }

    public void Dispose()
    {
        _basicShader.Dispose();
    }
}
