using System.Numerics;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Compiles, links, and manages a GLSL shader program.
/// </summary>
public sealed class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations = new();

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        var vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            var info = _gl.GetProgramInfoLog(_handle);
            throw new InvalidOperationException($"Shader link error: {info}");
        }

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use() => _gl.UseProgram(_handle);

    public void SetUniform(string name, int value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetUniform(string name, float value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetUniform(string name, Vector3 value)
    {
        _gl.Uniform3(GetLocation(name), value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        _gl.Uniform4(GetLocation(name), value.X, value.Y, value.Z, value.W);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        _gl.UniformMatrix4(GetLocation(name), 1, false, (float*)&value);
    }

    private int GetLocation(string name)
    {
        if (_uniformLocations.TryGetValue(name, out var location))
            return location;

        location = _gl.GetUniformLocation(_handle, name);
        _uniformLocations[name] = location;
        return location;
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
        {
            var info = _gl.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"{type} compile error: {info}");
        }

        return shader;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }
}
