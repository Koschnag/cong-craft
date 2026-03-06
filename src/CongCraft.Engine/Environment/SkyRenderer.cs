using CongCraft.Engine.Core;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Environment;

/// <summary>
/// Renders a full-screen gradient sky with procedural clouds and atmospheric scattering.
/// </summary>
public sealed class SkyRenderer : ISystem
{
    public int Priority => 35;

    private GL _gl = null!;
    private Shader _skyShader = null!;
    private Mesh _quad = null!;
    private DayNightCycle _dayNight = null!;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _dayNight = services.Get<DayNightCycle>();
        _skyShader = new Shader(_gl, ShaderSources.SkyVertex, ShaderSources.SkyFragment);
        _quad = PrimitiveMeshBuilder.CreateFullScreenQuad(_gl);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        _gl.DepthMask(false);
        _gl.Disable(EnableCap.DepthTest);

        _skyShader.Use();
        _skyShader.SetUniform("uZenithColor", _dayNight.ZenithColor);
        _skyShader.SetUniform("uHorizonColor", _dayNight.HorizonColor);
        _skyShader.SetUniform("uSunDirection", _dayNight.SunDirection);
        _skyShader.SetUniform("uTime", time.TotalTimeF);
        _quad.Draw();

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthMask(true);
    }

    public void Dispose()
    {
        _skyShader.Dispose();
        _quad.Dispose();
    }
}
