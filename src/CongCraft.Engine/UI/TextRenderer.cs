using System.Numerics;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.UI;

/// <summary>
/// Renders text using the BitmapFont atlas. Batches characters into a single draw call.
/// </summary>
public sealed class TextRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly BitmapFont _font;
    private readonly Shader _shader;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private const int MaxChars = 512;

    // Vertex: x, y, u, v (4 floats per vertex, 4 vertices per char)
    private readonly float[] _vertices = new float[MaxChars * 4 * 4];
    private readonly uint[] _indices = new uint[MaxChars * 6];

    public BitmapFont Font => _font;

    public TextRenderer(GL gl, BitmapFont font)
    {
        _gl = gl;
        _font = font;
        _shader = new Shader(gl, ShaderSources.TextVertex, ShaderSources.TextFragment);

        // Pre-compute index buffer (same pattern for every quad)
        for (int i = 0; i < MaxChars; i++)
        {
            uint b = (uint)(i * 4);
            _indices[i * 6 + 0] = b;
            _indices[i * 6 + 1] = b + 1;
            _indices[i * 6 + 2] = b + 2;
            _indices[i * 6 + 3] = b;
            _indices[i * 6 + 4] = b + 2;
            _indices[i * 6 + 5] = b + 3;
        }

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)),
                null, BufferUsageARB.DynamicDraw);
        }

        _ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (uint* p = _indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(_indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);
            }
        }

        // position (vec2)
        unsafe
        {
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
                (void*)(2 * sizeof(float)));
        }

        gl.BindVertexArray(0);
    }

    /// <summary>
    /// Draw a text string. Call between BeginBatch/EndBatch, or use directly.
    /// </summary>
    public void DrawText(string text, float x, float y, float scale, Vector4 color, Matrix4x4 projection)
    {
        if (string.IsNullOrEmpty(text)) return;

        int charCount = Math.Min(text.Length, MaxChars);
        float cursorX = x;
        // Use logical size for layout (8px advance), but render the higher-res glyph
        float charW = BitmapFont.LogicalWidth * scale;
        float charH = BitmapFont.LogicalHeight * scale;
        int vi = 0;

        for (int i = 0; i < charCount; i++)
        {
            char c = text[i];
            if (c == '\n')
            {
                cursorX = x;
                y -= charH + 2 * scale;
                continue;
            }

            var (u0, v0, u1, v1) = _font.GetGlyphUV(c);

            // Bottom-left
            _vertices[vi++] = cursorX;
            _vertices[vi++] = y;
            _vertices[vi++] = u0;
            _vertices[vi++] = v1;
            // Bottom-right
            _vertices[vi++] = cursorX + charW;
            _vertices[vi++] = y;
            _vertices[vi++] = u1;
            _vertices[vi++] = v1;
            // Top-right
            _vertices[vi++] = cursorX + charW;
            _vertices[vi++] = y + charH;
            _vertices[vi++] = u1;
            _vertices[vi++] = v0;
            // Top-left
            _vertices[vi++] = cursorX;
            _vertices[vi++] = y + charH;
            _vertices[vi++] = u0;
            _vertices[vi++] = v0;

            cursorX += charW;
        }

        int quads = vi / 16;
        if (quads == 0) return;

        _shader.Use();
        _shader.SetUniform("uProjection", projection);
        _shader.SetUniform("uColor", color);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _font.TextureId);
        _shader.SetUniform("uTexture", 0);

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* p = _vertices)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                    (nuint)(vi * sizeof(float)), p);
            }
        }

        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)(quads * 6),
                DrawElementsType.UnsignedInt, null);
        }
    }

    /// <summary>
    /// Measure text width in pixels at given scale.
    /// </summary>
    public static float MeasureWidth(string text, float scale)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int maxLineLen = 0;
        int lineLen = 0;
        foreach (char c in text)
        {
            if (c == '\n') { maxLineLen = Math.Max(maxLineLen, lineLen); lineLen = 0; }
            else lineLen++;
        }
        maxLineLen = Math.Max(maxLineLen, lineLen);
        return maxLineLen * BitmapFont.LogicalWidth * scale;
    }

    /// <summary>
    /// Word-wrap text to fit within maxWidthPx at the given scale.
    /// Inserts '\n' at word boundaries so lines don't exceed the width.
    /// </summary>
    public static string WrapText(string text, float maxWidthPx, float scale)
    {
        if (string.IsNullOrEmpty(text)) return text;
        float charW = BitmapFont.LogicalWidth * scale;
        int maxCharsPerLine = Math.Max(1, (int)(maxWidthPx / charW));

        var result = new System.Text.StringBuilder(text.Length + 20);
        int lineLen = 0;

        var words = text.Split(' ');
        for (int w = 0; w < words.Length; w++)
        {
            string word = words[w];

            // Handle explicit newlines within a word
            if (word.Contains('\n'))
            {
                var parts = word.Split('\n');
                for (int p = 0; p < parts.Length; p++)
                {
                    if (p > 0) { result.Append('\n'); lineLen = 0; }
                    if (lineLen + parts[p].Length > maxCharsPerLine && lineLen > 0)
                    {
                        result.Append('\n');
                        lineLen = 0;
                    }
                    if (lineLen > 0) { result.Append(' '); lineLen++; }
                    result.Append(parts[p]);
                    lineLen += parts[p].Length;
                }
                continue;
            }

            int needed = (lineLen > 0 ? 1 : 0) + word.Length;
            if (lineLen + needed > maxCharsPerLine && lineLen > 0)
            {
                result.Append('\n');
                lineLen = 0;
            }
            if (lineLen > 0) { result.Append(' '); lineLen++; }
            result.Append(word);
            lineLen += word.Length;
        }
        return result.ToString();
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _shader.Dispose();
    }
}
