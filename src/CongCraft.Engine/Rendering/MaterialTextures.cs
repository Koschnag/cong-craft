using CongCraft.Engine.Procedural;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Holds procedurally generated material textures for entity rendering.
/// Registered as a service so all render systems can share the same textures.
/// Textures are sampled via triplanar mapping in the entity shader.
/// </summary>
public sealed class MaterialTextures : IDisposable
{
    private readonly GL _gl;

    public uint MetalTex { get; }
    public uint LeatherTex { get; }
    public uint SkinTex { get; }
    public uint WoodTex { get; }
    public uint FabricTex { get; }

    public MaterialTextures(GL gl)
    {
        _gl = gl;

        // Generate and upload all material textures (512x512 with mipmaps)
        MetalTex = TextureGenerator.UploadTexture(gl, TextureGenerator.GenerateMetalPixels(), 512, 512);
        LeatherTex = TextureGenerator.UploadTexture(gl, TextureGenerator.GenerateLeatherPixels(), 512, 512);
        SkinTex = TextureGenerator.UploadTexture(gl, TextureGenerator.GenerateSkinPixels(), 512, 512);
        WoodTex = TextureGenerator.UploadTexture(gl, TextureGenerator.GenerateWoodPixels(), 512, 512);
        FabricTex = TextureGenerator.UploadTexture(gl, TextureGenerator.GenerateFabricPixels(), 512, 512);
    }

    /// <summary>
    /// Bind material textures to texture units for entity shaders.
    /// Shadow map is on slot 0, so materials start at slot 1.
    /// </summary>
    public void BindToShader(Shader shader)
    {
        _gl.ActiveTexture(TextureUnit.Texture1);
        _gl.BindTexture(TextureTarget.Texture2D, MetalTex);
        shader.SetUniform("uMetalTex", 1);

        _gl.ActiveTexture(TextureUnit.Texture2);
        _gl.BindTexture(TextureTarget.Texture2D, LeatherTex);
        shader.SetUniform("uLeatherTex", 2);

        _gl.ActiveTexture(TextureUnit.Texture3);
        _gl.BindTexture(TextureTarget.Texture2D, SkinTex);
        shader.SetUniform("uSkinTex", 3);

        _gl.ActiveTexture(TextureUnit.Texture4);
        _gl.BindTexture(TextureTarget.Texture2D, WoodTex);
        shader.SetUniform("uWoodTex", 4);

        _gl.ActiveTexture(TextureUnit.Texture5);
        _gl.BindTexture(TextureTarget.Texture2D, FabricTex);
        shader.SetUniform("uFabricTex", 5);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(MetalTex);
        _gl.DeleteTexture(LeatherTex);
        _gl.DeleteTexture(SkinTex);
        _gl.DeleteTexture(WoodTex);
        _gl.DeleteTexture(FabricTex);
    }
}
