using CongCraft.Engine.Core;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Loads PNG/JPG texture files from disk using StbImageSharp (AOT-safe).
/// Falls back to procedural generation when files are missing.
/// </summary>
public static class TextureLoader
{
    /// <summary>
    /// Try to load a texture from a PNG file. Returns 0 if file not found.
    /// </summary>
    public static uint LoadFromFile(GL gl, string path)
    {
        if (!File.Exists(path))
            return 0;

        try
        {
            StbImage.stbi_set_flip_vertically_on_load(1);

            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint texture = UploadRgba(gl, image.Data, image.Width, image.Height);
            DevLog.Info($"Loaded texture: {Path.GetFileName(path)} ({image.Width}x{image.Height})");
            return texture;
        }
        catch (Exception ex)
        {
            DevLog.Warn($"Failed to load texture {path}: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Load texture from file, fall back to procedural pixels if file missing.
    /// </summary>
    public static uint LoadOrGenerate(GL gl, string path, Func<byte[]> proceduralFallback,
        int proceduralSize = 512)
    {
        uint tex = LoadFromFile(gl, path);
        if (tex != 0) return tex;

        // Fall back to procedural
        var pixels = proceduralFallback();
        return TextureGenerator.UploadTexture(gl, pixels, proceduralSize, proceduralSize);
    }

    private static unsafe uint UploadRgba(GL gl, byte[] pixels, int width, int height)
    {
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        fixed (byte* p = pixels)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                (uint)width, (uint)height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }

        gl.GenerateMipmap(TextureTarget.Texture2D);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)GLEnum.Repeat);

        gl.GetFloat(GLEnum.MaxTextureMaxAnisotropy, out float maxAniso);
        if (maxAniso > 1f)
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy,
                MathF.Min(maxAniso, 16f));
        }

        return texture;
    }
}
