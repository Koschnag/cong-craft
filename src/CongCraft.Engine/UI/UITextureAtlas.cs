using CongCraft.Engine.Procedural;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.UI;

/// <summary>
/// Pre-generated UI texture handles. Created once at startup, shared across all UI systems.
/// Resource-efficient: small textures (64-256px), nearest/linear filtering.
/// </summary>
public sealed class UITextureAtlas : IDisposable
{
    public uint Parchment { get; private set; }
    public uint GoldLeaf { get; private set; }
    public uint Leather { get; private set; }
    public uint WoodGrain { get; private set; }
    public uint VineBorder { get; private set; }
    public uint CornerOrnament { get; private set; }
    public uint MenuBackground { get; private set; }
    public uint Metal { get; private set; }

    private readonly GL _gl;

    public UITextureAtlas(GL gl)
    {
        _gl = gl;

        Parchment = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateParchment(128), 128, 128);
        GoldLeaf = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateGoldLeaf(64), 64, 64);
        Leather = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateLeather(64), 64, 64);
        WoodGrain = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateWoodGrain(128), 128, 128);
        VineBorder = UploadWithAlpha(gl,
            UITextureGenerator.GenerateVineBorder(256, 32), 256, 32);
        CornerOrnament = UploadWithAlpha(gl,
            UITextureGenerator.GenerateCornerOrnament(64), 64, 64);
        MenuBackground = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateMenuBackground(512, 256), 512, 256);
        Metal = TextureGenerator.UploadTexture(gl,
            UITextureGenerator.GenerateMetal(64), 64, 64);
    }

    private static unsafe uint UploadWithAlpha(GL gl, byte[] pixels, int w, int h)
    {
        uint tex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);
        gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        fixed (byte* p = pixels)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)w, (uint)h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        return tex;
    }

    public void Dispose()
    {
        _gl.DeleteTexture(Parchment);
        _gl.DeleteTexture(GoldLeaf);
        _gl.DeleteTexture(Leather);
        _gl.DeleteTexture(WoodGrain);
        _gl.DeleteTexture(VineBorder);
        _gl.DeleteTexture(CornerOrnament);
        _gl.DeleteTexture(MenuBackground);
        _gl.DeleteTexture(Metal);
    }
}
