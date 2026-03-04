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

        // Render health bar for player (using HealthComponent if available, fallback to PlayerComponent)
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            float health, maxHealth;
            if (_world.HasComponent<HealthComponent>(entity))
            {
                var hc = _world.GetComponent<HealthComponent>(entity);
                health = hc.Current;
                maxHealth = hc.Max;

                // Damage flash: red border when hit
                if (hc.DamageFlashTimer > 0)
                {
                    DrawRect(new HudElement(new Vector2(0, 0), new Vector2(w, h),
                        new Vector4(0.5f, 0f, 0f, hc.DamageFlashTimer * 0.3f)));
                }
            }
            else
            {
                health = player.Health;
                maxHealth = player.MaxHealth;
            }

            var (bg, fill) = HealthBar.Calculate(health, maxHealth,
                new Vector2(20, 20), new Vector2(200, 20));

            DrawRect(bg);
            DrawRect(fill);

            // Combat HUD hints
            if (_world.HasComponent<CombatComponent>(entity))
            {
                var combat = _world.GetComponent<CombatComponent>(entity);

                // Block indicator (blue bar when blocking)
                if (combat.IsBlocking)
                {
                    DrawRect(new HudElement(new Vector2(20, 45), new Vector2(200, 5),
                        new Vector4(0.2f, 0.4f, 0.8f, 0.8f)));
                }

                // Dodge cooldown indicator
                if (combat.DodgeCooldownTimer > 0)
                {
                    float dodgeFill = 200f * (1f - combat.DodgeCooldownTimer / combat.DodgeCooldown);
                    DrawRect(new HudElement(new Vector2(20, 55), new Vector2(200, 4),
                        new Vector4(0.3f, 0.3f, 0.3f, 0.5f)));
                    DrawRect(new HudElement(new Vector2(20, 55), new Vector2(dodgeFill, 4),
                        new Vector4(0.8f, 0.8f, 0.2f, 0.7f)));
                }
            }
        }

        // Enemy HP bars (for nearby enemies)
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (!health.IsAlive || enemy.State == EnemyState.Dead) continue;
            if (!_world.HasComponent<TransformComponent>(entity)) continue;

            // Simple enemy count indicator at top
            // (World-space HP bars would need 3D-to-2D projection which is complex for HUD)
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
