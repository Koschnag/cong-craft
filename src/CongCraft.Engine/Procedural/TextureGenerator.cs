using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates high-quality procedural textures from noise — no image files needed.
/// 512x512 resolution with multi-octave detail and mipmaps.
/// </summary>
public static class TextureGenerator
{
    private const int DefaultSize = 512;

    public static byte[] GenerateGrassPixels(int size = DefaultSize, int seed = 42)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.04f);

        var detail = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        detail.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        detail.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        detail.SetFractalOctaves(5);
        detail.SetFrequency(0.12f);

        var blade = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        blade.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        blade.SetFrequency(0.10f);

        // Fine grain for close-up detail (Gothic 3/Two Worlds style)
        var fineGrain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 600);
        fineGrain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        fineGrain.SetFrequency(0.25f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float d = detail.GetNoise(x, y) * 0.5f + 0.5f;
            float bl = blade.GetNoise(x, y) * 0.5f + 0.5f;
            float fg = fineGrain.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.35f + d * 0.30f + bl * 0.20f + fg * 0.15f;

            // Richer, more varied greens with earthy undertones
            int i = (y * size + x) * 4;
            float warmth = n * 0.15f; // Slight warm patches
            pixels[i + 0] = (byte)Math.Clamp(30 + combined * 50 + warmth * 40, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(50 + combined * 115, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(10 + combined * 22, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    public static byte[] GenerateStonePixels(int size = DefaultSize, int seed = 123)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.03f);

        var crack = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 50);
        crack.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        crack.SetFrequency(0.05f);

        var roughness = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        roughness.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        roughness.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        roughness.SetFractalOctaves(6);
        roughness.SetFrequency(0.08f);

        // Vein/mineral detail layer
        var veins = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 700);
        veins.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        veins.SetFrequency(0.15f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float cr = crack.GetNoise(x, y) * 0.5f + 0.5f;
            float rough = roughness.GetNoise(x, y) * 0.5f + 0.5f;
            float vn = veins.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.35f + cr * 0.30f + rough * 0.25f + vn * 0.10f;

            // Warmer stone with slight color variation (like Gothic 3 mountain stone)
            int i = (y * size + x) * 4;
            float warmShift = vn * 8f;
            byte rVal = (byte)Math.Clamp(88 + combined * 92 + warmShift, 0, 255);
            byte gVal = (byte)Math.Clamp(84 + combined * 88, 0, 255);
            byte bVal = (byte)Math.Clamp(78 + combined * 82, 0, 255);
            pixels[i + 0] = rVal;
            pixels[i + 1] = gVal;
            pixels[i + 2] = bVal;
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    public static byte[] GenerateDirtPixels(int size = DefaultSize, int seed = 777)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFrequency(0.05f);

        var pebble = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 400);
        pebble.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        pebble.SetFrequency(0.06f);

        var grain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 500);
        grain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        grain.SetFrequency(0.2f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float p = pebble.GetNoise(x, y) * 0.5f + 0.5f;
            float g = grain.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.45f + p * 0.3f + g * 0.25f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = (byte)(80 + combined * 70);
            pixels[i + 1] = (byte)(55 + combined * 55);
            pixels[i + 2] = (byte)(28 + combined * 35);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Upload a texture with mipmaps for higher quality rendering at distance.
    /// </summary>
    public static unsafe uint UploadTexture(GL gl, byte[] pixels, int width, int height)
    {
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        fixed (byte* p = pixels)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                (uint)width, (uint)height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }

        // Generate mipmaps for quality at distance
        gl.GenerateMipmap(TextureTarget.Texture2D);

        // Trilinear filtering with anisotropic
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

        // Anisotropic filtering (best quality for terrain viewed at angles)
        gl.GetFloat(GLEnum.MaxTextureMaxAnisotropy, out float maxAniso);
        if (maxAniso > 1f)
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy,
                MathF.Min(maxAniso, 16f));
        }

        return texture;
    }
}
