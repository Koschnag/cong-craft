using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Renders all active particle emitters as colored point quads.
/// Uses additive blending for fire/spark effects.
/// </summary>
public sealed class ParticleRenderSystem : ISystem
{
    public int Priority => 103; // After main rendering, before HUD

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private Shader _particleShader = null!;
    private uint _vao, _vbo;

    // Shared list of emitters from other systems
    private static readonly List<ParticleEmitter> ActiveEmitters = new();

    public static void RegisterEmitter(ParticleEmitter emitter) => ActiveEmitters.Add(emitter);
    public static void UnregisterEmitter(ParticleEmitter emitter) => ActiveEmitters.Remove(emitter);

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _particleShader = new Shader(_gl, ShaderSources.ParticleVertex, ShaderSources.ParticleFragment);
        SetupQuad();
    }

    private unsafe void SetupQuad()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Simple quad: 2 triangles
        float[] vertices = {
            -0.5f, -0.5f,  0.5f, -0.5f,  0.5f, 0.5f,
            -0.5f, -0.5f,  0.5f,  0.5f, -0.5f, 0.5f
        };

        fixed (float* ptr = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
        }

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
        _gl.BindVertexArray(0);
    }

    public void Update(GameTime time)
    {
        float dt = time.DeltaTimeF;
        foreach (var emitter in ActiveEmitters)
            emitter.Update(dt);
    }

    public void Render(GameTime time)
    {
        if (ActiveEmitters.Count == 0) return;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // Additive

        _particleShader.Use();
        _particleShader.SetUniform("uView", _camera.ViewMatrix);
        _particleShader.SetUniform("uProjection", _camera.ProjectionMatrix);

        _gl.BindVertexArray(_vao);

        foreach (var emitter in ActiveEmitters)
        {
            foreach (ref readonly var p in emitter.Particles)
            {
                if (!p.IsAlive) continue;

                _particleShader.SetUniform("uPosition", p.Position);
                _particleShader.SetUniform("uSize", p.Size);
                _particleShader.SetUniform("uColor", p.Color);
                _particleShader.SetUniform("uCameraRight", _camera.Right);
                _particleShader.SetUniform("uCameraUp", Vector3.UnitY);

                _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
        }

        _gl.BindVertexArray(0);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Enable(EnableCap.DepthTest);
    }

    public void Dispose()
    {
        _particleShader.Dispose();
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }
}
