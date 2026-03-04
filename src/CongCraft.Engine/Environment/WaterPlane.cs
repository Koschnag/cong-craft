using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Environment;

/// <summary>
/// Renders a large semi-transparent water plane at a fixed Y level.
/// </summary>
public sealed class WaterPlane : ISystem
{
    public int Priority => 105; // Render after terrain

    private GL _gl = null!;
    private Shader _waterShader = null!;
    private Mesh _waterMesh = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;

    public float WaterLevel { get; set; } = 1.5f;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _waterShader = new Shader(_gl, ShaderSources.WaterVertex, ShaderSources.WaterFragment);
        _waterMesh = CreateWaterMesh(_gl, 400f);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _waterShader.Use();
        _waterShader.SetUniform("uView", _camera.ViewMatrix);
        _waterShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _waterShader.SetUniform("uModel", Matrix4x4.CreateTranslation(0, WaterLevel, 0));
        _waterShader.SetUniform("uTime", time.TotalTimeF);
        _waterShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_waterShader);

        _waterMesh.Draw();

        _gl.Disable(EnableCap.Blend);
    }

    private static Mesh CreateWaterMesh(GL gl, float size)
    {
        float h = size / 2f;
        int divisions = 40;
        float step = size / divisions;

        var verts = new List<float>();
        var inds = new List<uint>();

        for (int z = 0; z <= divisions; z++)
        for (int x = 0; x <= divisions; x++)
        {
            verts.Add(-h + x * step); // x
            verts.Add(0);              // y (modified in shader)
            verts.Add(-h + z * step); // z
        }

        for (int z = 0; z < divisions; z++)
        for (int x = 0; x < divisions; x++)
        {
            uint tl = (uint)(z * (divisions + 1) + x);
            uint tr = tl + 1;
            uint bl = tl + (uint)(divisions + 1);
            uint br = bl + 1;
            inds.AddRange(new[] { tl, bl, br, tl, br, tr });
        }

        return new Mesh(gl, verts.ToArray(), inds.ToArray(), VertexLayout.Position3D);
    }

    public void Dispose()
    {
        _waterShader.Dispose();
        _waterMesh.Dispose();
    }
}
