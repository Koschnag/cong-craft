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
    public int Priority => 95;

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
        // Sky vertex shader sets Z=0.999 (near far plane) so depth test against
        // already-drawn terrain (Z < 0.999) correctly clips the sky behind geometry.
        // DepthMask=false prevents the sky from overwriting terrain depth values.
        _gl.DepthMask(false);

        _skyShader.Use();
        _skyShader.SetUniform("uZenithColor", _dayNight.ZenithColor);
        _skyShader.SetUniform("uHorizonColor", _dayNight.HorizonColor);
        _skyShader.SetUniform("uSunDirection", _dayNight.SunDirection);
        _skyShader.SetUniform("uTime", time.TotalTimeF);
        _quad.Draw();

        _gl.DepthMask(true);
    }

    public void Dispose()
    {
        _skyShader.Dispose();
        _quad.Dispose();
    }
}
