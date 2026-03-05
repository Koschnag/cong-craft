using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates procedural textures from noise — no image files needed.
/// </summary>
public static class TextureGenerator
{
    public static byte[] GenerateGrassPixels(int size = 256, int seed = 42)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.05f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
                int i = (y * size + x) * 4;
                pixels[i + 0] = (byte)(30 + n * 50);
                pixels[i + 1] = (byte)(70 + n * 100);
                pixels[i + 2] = (byte)(15 + n * 30);
                pixels[i + 3] = 255;
            }
        return pixels;
    }

    public static byte[] GenerateStonePixels(int size = 256, int seed = 123)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.04f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
                int i = (y * size + x) * 4;
                byte v = (byte)(100 + n * 80);
                pixels[i + 0] = v;
                pixels[i + 1] = (byte)(v - 5);
                pixels[i + 2] = (byte)(v - 10);
                pixels[i + 3] = 255;
            }
        return pixels;
    }

    public static byte[] GenerateDirtPixels(int size = 256, int seed = 777)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(4);
        noise.SetFrequency(0.06f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
                int i = (y * size + x) * 4;
                pixels[i + 0] = (byte)(90 + n * 60);
                pixels[i + 1] = (byte)(60 + n * 50);
                pixels[i + 2] = (byte)(30 + n * 30);
                pixels[i + 3] = 255;
            }
        return pixels;
    }

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

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

        return texture;
    }
}
