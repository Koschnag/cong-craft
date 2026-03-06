using CongCraft.Engine.Core;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// HDR framebuffer + bloom + tonemapping post-processing pipeline.
/// Renders the scene to an FBO, extracts bright pixels, blurs them,
/// then composites with tonemapping and vignette.
/// </summary>
public sealed class PostProcessing : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _bloomExtract;
    private readonly Shader _bloomBlur;
    private readonly Shader _toneMap;
    private readonly Mesh _quad;

    // Main scene FBO (HDR)
    private uint _sceneFbo, _sceneColorTex, _sceneDepthRbo;

    // Bloom ping-pong FBOs
    private uint _bloomFbo1, _bloomTex1;
    private uint _bloomFbo2, _bloomTex2;

    private int _width, _height;

    public float BloomThreshold { get; set; } = 0.72f;  // Lower = more objects glow (torches, magic)
    public float BloomIntensity { get; set; } = 0.50f;  // Stronger glow for SpellForce atmosphere
    public float Exposure { get; set; } = 0.85f;        // Darker overall — Gothic/SpellForce moodiness

    public PostProcessing(GL gl, int width, int height)
    {
        _gl = gl;
        _width = width;
        _height = height;

        _bloomExtract = new Shader(gl, ShaderSources.PostProcessVertex, ShaderSources.BloomExtractFragment);
        _bloomBlur = new Shader(gl, ShaderSources.PostProcessVertex, ShaderSources.BloomBlurFragment);
        _toneMap = new Shader(gl, ShaderSources.PostProcessVertex, ShaderSources.ToneMappingFragment);

        float[] vertices = { -1, -1, 1, -1, 1, 1, -1, 1 };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };
        _quad = new Mesh(gl, vertices, indices, VertexLayout.Position2D);

        CreateFramebuffers();
    }

    private void CreateFramebuffers()
    {
        // Scene FBO (full res, HDR float16)
        _sceneFbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _sceneFbo);

        _sceneColorTex = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _sceneColorTex);
        unsafe
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f,
                (uint)_width, (uint)_height, 0, PixelFormat.Rgba, PixelType.Float, null);
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _sceneColorTex, 0);

        _sceneDepthRbo = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _sceneDepthRbo);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24,
            (uint)_width, (uint)_height);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _sceneDepthRbo);

        CheckFramebufferComplete("Scene");
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Bloom FBOs (half res)
        int bw = _width / 2, bh = _height / 2;
        CreateBloomFbo(out _bloomFbo1, out _bloomTex1, bw, bh, "Bloom1");
        CreateBloomFbo(out _bloomFbo2, out _bloomTex2, bw, bh, "Bloom2");
    }

    private void CreateBloomFbo(out uint fbo, out uint tex, int w, int h, string name)
    {
        fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        tex = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, tex);
        unsafe
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f,
                (uint)w, (uint)h, 0, PixelFormat.Rgba, PixelType.Float, null);
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, tex, 0);
        CheckFramebufferComplete(name);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void CheckFramebufferComplete(string name)
    {
        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
            DevLog.Warn($"PostProcessing: {name} FBO incomplete — status {status}");
    }

    /// <summary>
    /// Begin rendering the scene into the HDR FBO.
    /// </summary>
    public void BeginScene()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _sceneFbo);
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    /// <summary>
    /// End scene rendering and apply post-processing (bloom + tonemapping).
    /// </summary>
    public void EndSceneAndApply()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        int bw = _width / 2, bh = _height / 2;

        // 1. Extract bright pixels
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _bloomFbo1);
        _gl.Viewport(0, 0, (uint)bw, (uint)bh);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        _bloomExtract.Use();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneColorTex);
        _bloomExtract.SetUniform("uScene", 0);
        _bloomExtract.SetUniform("uThreshold", BloomThreshold);
        _gl.Disable(EnableCap.DepthTest);
        _quad.Draw();

        // 2. Ping-pong Gaussian blur (4 passes)
        bool horizontal = true;
        _bloomBlur.Use();
        for (int i = 0; i < 4; i++)
        {
            uint targetFbo = horizontal ? _bloomFbo2 : _bloomFbo1;
            uint sourceTex = horizontal ? _bloomTex1 : _bloomTex2;

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, targetFbo);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, sourceTex);
            _bloomBlur.SetUniform("uImage", 0);
            _bloomBlur.SetUniform("uHorizontal", horizontal ? 1 : 0);
            _quad.Draw();

            horizontal = !horizontal;
        }

        // 3. Final compositing with tonemapping
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _toneMap.Use();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneColorTex);
        _toneMap.SetUniform("uScene", 0);

        _gl.ActiveTexture(TextureUnit.Texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _bloomTex1); // Last blur output
        _toneMap.SetUniform("uBloom", 1);

        _toneMap.SetUniform("uBloomIntensity", BloomIntensity);
        _toneMap.SetUniform("uExposure", Exposure);
        _quad.Draw();

        _gl.Enable(EnableCap.DepthTest);
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;

        // Cleanup old
        _gl.DeleteFramebuffer(_sceneFbo);
        _gl.DeleteTexture(_sceneColorTex);
        _gl.DeleteRenderbuffer(_sceneDepthRbo);
        _gl.DeleteFramebuffer(_bloomFbo1);
        _gl.DeleteTexture(_bloomTex1);
        _gl.DeleteFramebuffer(_bloomFbo2);
        _gl.DeleteTexture(_bloomTex2);

        CreateFramebuffers();
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(_sceneFbo);
        _gl.DeleteTexture(_sceneColorTex);
        _gl.DeleteRenderbuffer(_sceneDepthRbo);
        _gl.DeleteFramebuffer(_bloomFbo1);
        _gl.DeleteTexture(_bloomTex1);
        _gl.DeleteFramebuffer(_bloomFbo2);
        _gl.DeleteTexture(_bloomTex2);
        _bloomExtract.Dispose();
        _bloomBlur.Dispose();
        _toneMap.Dispose();
        _quad.Dispose();
    }
}
