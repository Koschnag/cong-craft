using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class TextureGeneratorTests
{
    [Fact]
    public void GenerateGrassPixels_CorrectSize()
    {
        var pixels = TextureGenerator.GenerateGrassPixels(size: 64);
        Assert.Equal(64 * 64 * 4, pixels.Length);
    }

    [Fact]
    public void GenerateStonePixels_CorrectSize()
    {
        var pixels = TextureGenerator.GenerateStonePixels(size: 128);
        Assert.Equal(128 * 128 * 4, pixels.Length);
    }

    [Fact]
    public void GenerateDirtPixels_CorrectSize()
    {
        var pixels = TextureGenerator.GenerateDirtPixels(size: 32);
        Assert.Equal(32 * 32 * 4, pixels.Length);
    }

    [Fact]
    public void GenerateGrassPixels_AllAlphaOpaque()
    {
        var pixels = TextureGenerator.GenerateGrassPixels(size: 32);
        for (int i = 3; i < pixels.Length; i += 4)
        {
            Assert.Equal(255, pixels[i]);
        }
    }

    [Fact]
    public void GenerateGrassPixels_GreenDominates()
    {
        var pixels = TextureGenerator.GenerateGrassPixels(size: 64);
        long totalR = 0, totalG = 0, totalB = 0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            totalR += pixels[i];
            totalG += pixels[i + 1];
            totalB += pixels[i + 2];
        }
        Assert.True(totalG > totalR, "Green channel should dominate for grass");
        Assert.True(totalG > totalB, "Green channel should dominate for grass");
    }

    [Fact]
    public void GenerateGrassPixels_Deterministic()
    {
        var p1 = TextureGenerator.GenerateGrassPixels(size: 32, seed: 42);
        var p2 = TextureGenerator.GenerateGrassPixels(size: 32, seed: 42);
        Assert.Equal(p1, p2);
    }
}
