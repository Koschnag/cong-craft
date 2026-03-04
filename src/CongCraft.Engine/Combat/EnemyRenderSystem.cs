using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Combat;

/// <summary>
/// Renders enemies and their HP bars.
/// </summary>
public sealed class EnemyRenderSystem : ISystem
{
    public int Priority => 101; // After main render

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _basicShader = null!;

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

        foreach (var (entity, enemy, meshComp) in _world.Query<EnemyComponent, MeshRendererComponent>())
        {
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);

            // Dead enemies fade out (scale down)
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
