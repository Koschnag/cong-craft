using Silk.NET.OpenGL;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Wraps OpenGL VAO/VBO/EBO for rendering geometry.
/// </summary>
public sealed class Mesh : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly uint _indexCount;

    public uint IndexCount => _indexCount;

    public unsafe Mesh(GL gl, float[] vertices, uint[] indices, VertexLayout layout)
    {
        _gl = gl;
        _indexCount = (uint)indices.Length;

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* i = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }

        layout.Apply(gl);

        _gl.BindVertexArray(0);
    }

    public void Draw(PrimitiveType mode = PrimitiveType.Triangles)
    {
        _gl.BindVertexArray(_vao);
        unsafe
        {
            _gl.DrawElements(mode, _indexCount, DrawElementsType.UnsignedInt, null);
        }
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}

/// <summary>
/// Describes vertex attribute layout for a mesh.
/// </summary>
public sealed class VertexLayout
{
    private readonly List<(uint Index, int Size, uint Offset)> _attributes = new();
    private uint _stride;

    public VertexLayout(uint stride)
    {
        _stride = stride;
    }

    public VertexLayout Add(uint index, int size, uint offset)
    {
        _attributes.Add((index, size, offset));
        return this;
    }

    public unsafe void Apply(GL gl)
    {
        foreach (var (index, size, offset) in _attributes)
        {
            gl.EnableVertexAttribArray(index);
            gl.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false,
                _stride, (void*)offset);
        }
    }

    // Common layouts
    public static VertexLayout PositionNormalTexCoord =>
        new VertexLayout(8 * sizeof(float))
            .Add(0, 3, 0)                       // position
            .Add(1, 3, 3 * sizeof(float))        // normal
            .Add(2, 2, 6 * sizeof(float));       // texcoord

    public static VertexLayout PositionNormalColor =>
        new VertexLayout(9 * sizeof(float))
            .Add(0, 3, 0)                        // position
            .Add(1, 3, 3 * sizeof(float))        // normal
            .Add(2, 3, 6 * sizeof(float));       // color

    public static VertexLayout Position2D =>
        new VertexLayout(2 * sizeof(float))
            .Add(0, 2, 0);                       // position

    public static VertexLayout Position3D =>
        new VertexLayout(3 * sizeof(float))
            .Add(0, 3, 0);                       // position
}
