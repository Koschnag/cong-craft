using CongCraft.Engine.Procedural;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Holds material textures for entity rendering.
/// Tries to load from congcraft_assets PNG files first, falls back to procedural.
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

        // Load material textures: try PNG files, fall back to procedural (512x512 with mipmaps)
        MetalTex = TextureLoader.LoadOrGenerate(gl,
            AssetPaths.TextureFile("materials/mat_metal.png"),
            () => TextureGenerator.GenerateMetalPixels());

        LeatherTex = TextureLoader.LoadOrGenerate(gl,
            AssetPaths.TextureFile("materials/mat_leather.png"),
            () => TextureGenerator.GenerateLeatherPixels());

        SkinTex = TextureLoader.LoadOrGenerate(gl,
            AssetPaths.TextureFile("materials/mat_skin.png"),
            () => TextureGenerator.GenerateSkinPixels());

        WoodTex = TextureLoader.LoadOrGenerate(gl,
            AssetPaths.TextureFile("materials/mat_wood.png"),
            () => TextureGenerator.GenerateWoodPixels());

        FabricTex = TextureLoader.LoadOrGenerate(gl,
            AssetPaths.TextureFile("materials/mat_fabric.png"),
            () => TextureGenerator.GenerateFabricPixels());
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
