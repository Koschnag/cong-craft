using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.UI;

/// <summary>
/// Renders 2D HUD overlay using orthographic projection.
/// </summary>
public sealed class HudSystem : ISystem
{
    public int Priority => 110;

    private GL _gl = null!;
    private World _world = null!;
    private Shader _hudShader = null!;
    private Mesh _quad = null!;
    private IWindow _window = null!;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _window = services.Get<IWindow>();
        _hudShader = new Shader(_gl, ShaderSources.HudVertex, ShaderSources.HudFragment);
        _quad = PrimitiveMeshBuilder.CreateUnitQuad(_gl);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        int w = _window.Size.X, h = _window.Size.Y;
        var ortho = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, -1, 1);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        // Render health bar for player
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            var (bg, fill) = HealthBar.Calculate(
                player.Health, player.MaxHealth,
                new Vector2(20, 20), new Vector2(200, 20));

            DrawRect(bg);
            DrawRect(fill);
        }

        // Minimap placeholder (top-right)
        DrawRect(new HudElement(
            new Vector2(w - 170, h - 170),
            new Vector2(150, 150),
            new Vector4(0.1f, 0.1f, 0.1f, 0.5f)));

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void DrawRect(HudElement element)
    {
        _hudShader.SetUniform("uRect", new Vector4(
            element.Position.X, element.Position.Y,
            element.Size.X, element.Size.Y));
        _hudShader.SetUniform("uColor", element.Color);
        _quad.Draw();
    }

    public void Dispose()
    {
        _hudShader.Dispose();
        _quad.Dispose();
    }
}
