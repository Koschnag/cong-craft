using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Tests.Rendering;

public class ShaderSourcesTests
{
    [Fact]
    public void ParticleFragment_DoesNotUseGlPointCoord()
    {
        // gl_PointCoord is only valid for GL_POINTS, not for triangle-based quads.
        // Using it causes undefined behavior (opaque white squares).
        Assert.DoesNotContain("gl_PointCoord", ShaderSources.ParticleFragment);
    }

    [Fact]
    public void ParticleShaders_UseVaryingQuadCoord()
    {
        // Particle vertex shader must output vQuadCoord for the fragment shader
        Assert.Contains("vQuadCoord", ShaderSources.ParticleVertex);
        Assert.Contains("vQuadCoord", ShaderSources.ParticleFragment);
    }

    [Fact]
    public void TerrainFragment_GrassRangeCoversZeroHeight()
    {
        // The terrain shader must show grass at height 0 (spawn area).
        // Grass range should include 0 — verify the shader doesn't start grass at a positive height.
        // The shader uses: "else if (Height < 10.0) baseColor = grassSample;"
        // and the previous branch is: "else if (Height < -2.0)"
        // So grass covers -2 to 10 which includes 0.
        Assert.Contains("Height < 10.0", ShaderSources.TerrainFragment);
        Assert.Contains("grassSample", ShaderSources.TerrainFragment);
    }

    [Fact]
    public void TerrainFragment_SnowOnlyAtHighAltitude()
    {
        // Snow should only appear at extreme heights (16+), not at typical terrain heights
        Assert.Contains("smoothstep(16.0, 22.0, Height)", ShaderSources.TerrainFragment);
        Assert.Contains("smoothstep(16.0, 20.0, Height)", ShaderSources.TerrainFragment);
    }
}
