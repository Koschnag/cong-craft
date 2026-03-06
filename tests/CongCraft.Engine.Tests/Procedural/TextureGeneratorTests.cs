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

    [Theory]
    [InlineData(nameof(TextureGenerator.GenerateMetalPixels))]
    [InlineData(nameof(TextureGenerator.GenerateLeatherPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSkinPixels))]
    [InlineData(nameof(TextureGenerator.GenerateWoodPixels))]
    [InlineData(nameof(TextureGenerator.GenerateFabricPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSnowPixels))]
    [InlineData(nameof(TextureGenerator.GeneratePathPixels))]
    public void MaterialTexture_CorrectSize(string methodName)
    {
        int size = 64;
        var pixels = methodName switch
        {
            nameof(TextureGenerator.GenerateMetalPixels) => TextureGenerator.GenerateMetalPixels(size),
            nameof(TextureGenerator.GenerateLeatherPixels) => TextureGenerator.GenerateLeatherPixels(size),
            nameof(TextureGenerator.GenerateSkinPixels) => TextureGenerator.GenerateSkinPixels(size),
            nameof(TextureGenerator.GenerateWoodPixels) => TextureGenerator.GenerateWoodPixels(size),
            nameof(TextureGenerator.GenerateFabricPixels) => TextureGenerator.GenerateFabricPixels(size),
            nameof(TextureGenerator.GenerateSnowPixels) => TextureGenerator.GenerateSnowPixels(size),
            nameof(TextureGenerator.GeneratePathPixels) => TextureGenerator.GeneratePathPixels(size),
            _ => throw new ArgumentException($"Unknown texture: {methodName}")
        };
        Assert.Equal(size * size * 4, pixels.Length);
    }

    [Theory]
    [InlineData(nameof(TextureGenerator.GenerateMetalPixels))]
    [InlineData(nameof(TextureGenerator.GenerateLeatherPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSkinPixels))]
    [InlineData(nameof(TextureGenerator.GenerateWoodPixels))]
    [InlineData(nameof(TextureGenerator.GenerateFabricPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSnowPixels))]
    [InlineData(nameof(TextureGenerator.GeneratePathPixels))]
    public void MaterialTexture_AllAlphaOpaque(string methodName)
    {
        int size = 32;
        var pixels = methodName switch
        {
            nameof(TextureGenerator.GenerateMetalPixels) => TextureGenerator.GenerateMetalPixels(size),
            nameof(TextureGenerator.GenerateLeatherPixels) => TextureGenerator.GenerateLeatherPixels(size),
            nameof(TextureGenerator.GenerateSkinPixels) => TextureGenerator.GenerateSkinPixels(size),
            nameof(TextureGenerator.GenerateWoodPixels) => TextureGenerator.GenerateWoodPixels(size),
            nameof(TextureGenerator.GenerateFabricPixels) => TextureGenerator.GenerateFabricPixels(size),
            nameof(TextureGenerator.GenerateSnowPixels) => TextureGenerator.GenerateSnowPixels(size),
            nameof(TextureGenerator.GeneratePathPixels) => TextureGenerator.GeneratePathPixels(size),
            _ => throw new ArgumentException($"Unknown texture: {methodName}")
        };
        for (int i = 3; i < pixels.Length; i += 4)
            Assert.Equal(255, pixels[i]);
    }

    [Theory]
    [InlineData(nameof(TextureGenerator.GenerateGrassPixels))]
    [InlineData(nameof(TextureGenerator.GenerateStonePixels))]
    [InlineData(nameof(TextureGenerator.GenerateDirtPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSnowPixels))]
    [InlineData(nameof(TextureGenerator.GeneratePathPixels))]
    [InlineData(nameof(TextureGenerator.GenerateMetalPixels))]
    [InlineData(nameof(TextureGenerator.GenerateLeatherPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSkinPixels))]
    [InlineData(nameof(TextureGenerator.GenerateWoodPixels))]
    [InlineData(nameof(TextureGenerator.GenerateFabricPixels))]
    public void Texture_HasColorVariation_NotUniform(string methodName)
    {
        int size = 64;
        var pixels = GenerateByName(methodName, size);

        // Collect unique R values to verify the texture has actual variation
        var uniqueR = new HashSet<byte>();
        var uniqueG = new HashSet<byte>();
        for (int i = 0; i < pixels.Length; i += 4)
        {
            uniqueR.Add(pixels[i]);
            uniqueG.Add(pixels[i + 1]);
        }

        // A real texture should have many distinct color values, not uniform
        Assert.True(uniqueR.Count > 10, $"{methodName}: R channel has only {uniqueR.Count} unique values — texture appears uniform");
        Assert.True(uniqueG.Count > 10, $"{methodName}: G channel has only {uniqueG.Count} unique values — texture appears uniform");
    }

    [Theory]
    [InlineData(nameof(TextureGenerator.GenerateGrassPixels))]
    [InlineData(nameof(TextureGenerator.GenerateStonePixels))]
    [InlineData(nameof(TextureGenerator.GenerateDirtPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSnowPixels))]
    [InlineData(nameof(TextureGenerator.GeneratePathPixels))]
    [InlineData(nameof(TextureGenerator.GenerateMetalPixels))]
    [InlineData(nameof(TextureGenerator.GenerateLeatherPixels))]
    [InlineData(nameof(TextureGenerator.GenerateSkinPixels))]
    [InlineData(nameof(TextureGenerator.GenerateWoodPixels))]
    [InlineData(nameof(TextureGenerator.GenerateFabricPixels))]
    public void Texture_NoPixelIsBlack_AllChannelsNonZero(string methodName)
    {
        int size = 64;
        var pixels = GenerateByName(methodName, size);

        // Every pixel should have non-zero color (textures are meant to be visible)
        for (int i = 0; i < pixels.Length; i += 4)
        {
            int r = pixels[i], g = pixels[i + 1], b = pixels[i + 2];
            Assert.True(r + g + b > 0, $"{methodName}: pixel at offset {i / 4} is fully black (0,0,0)");
        }
    }

    private static byte[] GenerateByName(string methodName, int size) => methodName switch
    {
        nameof(TextureGenerator.GenerateGrassPixels) => TextureGenerator.GenerateGrassPixels(size),
        nameof(TextureGenerator.GenerateStonePixels) => TextureGenerator.GenerateStonePixels(size),
        nameof(TextureGenerator.GenerateDirtPixels) => TextureGenerator.GenerateDirtPixels(size),
        nameof(TextureGenerator.GenerateSnowPixels) => TextureGenerator.GenerateSnowPixels(size),
        nameof(TextureGenerator.GeneratePathPixels) => TextureGenerator.GeneratePathPixels(size),
        nameof(TextureGenerator.GenerateMetalPixels) => TextureGenerator.GenerateMetalPixels(size),
        nameof(TextureGenerator.GenerateLeatherPixels) => TextureGenerator.GenerateLeatherPixels(size),
        nameof(TextureGenerator.GenerateSkinPixels) => TextureGenerator.GenerateSkinPixels(size),
        nameof(TextureGenerator.GenerateWoodPixels) => TextureGenerator.GenerateWoodPixels(size),
        nameof(TextureGenerator.GenerateFabricPixels) => TextureGenerator.GenerateFabricPixels(size),
        _ => throw new ArgumentException($"Unknown texture: {methodName}")
    };
}
