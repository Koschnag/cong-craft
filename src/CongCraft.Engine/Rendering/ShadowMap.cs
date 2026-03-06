using System.Numerics;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Directional shadow map using a depth-only FBO.
/// Resolution: 2048x2048 for quality shadows.
/// </summary>
public sealed class ShadowMap : IDisposable
{
    private readonly GL _gl;
    private readonly uint _fbo;
    private readonly uint _depthTexture;
    private readonly Shader _shadowShader;
    private readonly Shader _shadowShaderEntity;
    public const int Resolution = 2048;
    public const float OrthoSize = 80f;
    public const float NearPlane = 0.1f;
    public const float FarPlane = 200f;

    public uint DepthTexture => _depthTexture;
    public Matrix4x4 LightSpaceMatrix { get; private set; } = Matrix4x4.Identity;

    public ShadowMap(GL gl)
    {
        _gl = gl;
        _shadowShader = new Shader(gl, ShaderSources.ShadowVertex, ShaderSources.ShadowFragment);
        _shadowShaderEntity = new Shader(gl, ShaderSources.ShadowVertexEntity, ShaderSources.ShadowFragment);

        // Create depth texture
        _depthTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);
        unsafe
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent24,
                Resolution, Resolution, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToBorder);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToBorder);
        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        unsafe
        {
            fixed (float* ptr = borderColor)
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, ptr);
        }

        // Create FBO
        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, _depthTexture, 0);
        _gl.DrawBuffer(DrawBufferMode.None);
        _gl.ReadBuffer(ReadBufferMode.None);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>
    /// Update the light space matrix based on sun direction and camera target.
    /// </summary>
    public void UpdateLightMatrix(Vector3 sunDirection, Vector3 cameraTarget)
    {
        // Position the light looking at the scene center
        var lightPos = cameraTarget - Vector3.Normalize(sunDirection) * 60f;
        var lightView = Matrix4x4.CreateLookAt(lightPos, cameraTarget, Vector3.UnitY);
        var lightProjection = Matrix4x4.CreateOrthographicOffCenter(
            -OrthoSize, OrthoSize, -OrthoSize, OrthoSize, NearPlane, FarPlane);
        LightSpaceMatrix = lightView * lightProjection;
    }

    /// <summary>
    /// Begin shadow pass: bind FBO and clear depth.
    /// </summary>
    public void BeginPass()
    {
        _gl.Viewport(0, 0, Resolution, Resolution);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.Clear((uint)ClearBufferMask.DepthBufferBit);
        _gl.Enable(EnableCap.DepthTest);
        _gl.CullFace(TriangleFace.Front); // Reduce peter panning
    }

    /// <summary>
    /// Get the shadow shader for terrain (Position + Normal + TexCoord layout).
    /// </summary>
    public Shader GetTerrainShader()
    {
        _shadowShader.Use();
        _shadowShader.SetUniform("uLightSpaceMatrix", LightSpaceMatrix);
        _shadowShader.SetUniform("uModel", Matrix4x4.Identity);
        return _shadowShader;
    }

    /// <summary>
    /// Get the shadow shader for entities (Position + Normal + Color layout).
    /// </summary>
    public Shader GetEntityShader()
    {
        _shadowShaderEntity.Use();
        _shadowShaderEntity.SetUniform("uLightSpaceMatrix", LightSpaceMatrix);
        return _shadowShaderEntity;
    }

    /// <summary>
    /// End shadow pass: restore default FBO and viewport.
    /// </summary>
    public void EndPass(int viewportWidth, int viewportHeight)
    {
        _gl.CullFace(TriangleFace.Back);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)viewportWidth, (uint)viewportHeight);
    }

    /// <summary>
    /// Bind the shadow depth texture to a texture unit and set uniforms on a shader.
    /// </summary>
    public void BindToShader(Shader shader, int textureUnit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);
        shader.SetUniform("uShadowMap", textureUnit);
        shader.SetUniform("uLightSpaceMatrix", LightSpaceMatrix);
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(_fbo);
        _gl.DeleteTexture(_depthTexture);
        _shadowShader.Dispose();
        _shadowShaderEntity.Dispose();
    }
}
