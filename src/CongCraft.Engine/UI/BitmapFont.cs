using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.UI;

/// <summary>
/// Procedural pixel-art bitmap font. Generates an 8x8 glyph atlas at startup.
/// All glyphs are hardcoded — no external font files needed.
/// Covers ASCII 32-126 (printable characters).
/// </summary>
public sealed class BitmapFont : IDisposable
{
    public const int GlyphWidth = 8;
    public const int GlyphHeight = 8;
    public const int FirstChar = 32;
    public const int LastChar = 126;
    public const int CharCount = LastChar - FirstChar + 1;
    public const int AtlasCols = 16;
    public const int AtlasRows = (CharCount + AtlasCols - 1) / AtlasCols;

    public int AtlasWidth => AtlasCols * GlyphWidth;
    public int AtlasHeight => AtlasRows * GlyphHeight;

    public uint TextureId { get; private set; }
    private readonly GL _gl;

    public BitmapFont(GL gl)
    {
        _gl = gl;
        var pixels = GenerateAtlas();
        TextureId = UploadAtlas(pixels);
    }

    /// <summary>
    /// Gets UV coordinates for a character. Returns (u0, v0, u1, v1).
    /// </summary>
    public (float U0, float V0, float U1, float V1) GetGlyphUV(char c)
    {
        int index = c - FirstChar;
        if (index < 0 || index >= CharCount)
            index = '?' - FirstChar;

        int col = index % AtlasCols;
        int row = index / AtlasCols;

        float u0 = (float)(col * GlyphWidth) / AtlasWidth;
        float v0 = (float)(row * GlyphHeight) / AtlasHeight;
        float u1 = (float)((col + 1) * GlyphWidth) / AtlasWidth;
        float v1 = (float)((row + 1) * GlyphHeight) / AtlasHeight;

        return (u0, v0, u1, v1);
    }

    private byte[] GenerateAtlas()
    {
        var pixels = new byte[AtlasWidth * AtlasHeight * 4];

        for (int i = 0; i < CharCount; i++)
        {
            char c = (char)(FirstChar + i);
            ulong glyph = GetGlyphBitmap(c);
            int col = i % AtlasCols;
            int row = i / AtlasCols;

            for (int gy = 0; gy < GlyphHeight; gy++)
            {
                for (int gx = 0; gx < GlyphWidth; gx++)
                {
                    int bit = gy * GlyphWidth + gx;
                    bool set = ((glyph >> (63 - bit)) & 1) == 1;

                    int px = col * GlyphWidth + gx;
                    int py = row * GlyphHeight + gy;
                    int idx = (py * AtlasWidth + px) * 4;

                    if (set)
                    {
                        pixels[idx + 0] = 255;
                        pixels[idx + 1] = 255;
                        pixels[idx + 2] = 255;
                        pixels[idx + 3] = 255;
                    }
                    // else remains 0,0,0,0 (transparent)
                }
            }
        }

        return pixels;
    }

    private unsafe uint UploadAtlas(byte[] pixels)
    {
        uint tex = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, tex);

        fixed (byte* p = pixels)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                (uint)AtlasWidth, (uint)AtlasHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        return tex;
    }

    /// <summary>
    /// Returns a 64-bit bitmap for an 8x8 glyph. Each bit = one pixel, row-major from top-left.
    /// Medieval-styled pixel font with slight serifs.
    /// </summary>
    private static ulong GetGlyphBitmap(char c) => c switch
    {
        ' ' => 0UL,
        '!' => 0x1818181818001800UL,
        '"' => 0x6666660000000000UL,
        '#' => 0x00367F36367F3600UL,
        '$' => 0x083E280E023C0800UL,
        '%' => 0x6266081020464600UL,
        '&' => 0x1C36361A6C663B00UL,
        '\'' => 0x0C0C060000000000UL,
        '(' => 0x0C18303030180C00UL,
        ')' => 0x300C06060C180C30UL,
        '*' => 0x00663CFF3C660000UL,
        '+' => 0x0018187E18180000UL,
        ',' => 0x0000000000181830UL,
        '-' => 0x000000007E000000UL,
        '.' => 0x0000000000181800UL,
        '/' => 0x0206081020400000UL,
        // Numbers - medieval style with subtle serifs
        '0' => 0x3C666E7666663C00UL,
        '1' => 0x1838181818187E00UL,
        '2' => 0x3C660C1830607E00UL,
        '3' => 0x3C660C1C06663C00UL,
        '4' => 0x0C1C2C4C7E0C0C00UL,
        '5' => 0x7E607C0606663C00UL,
        '6' => 0x1C30607C66663C00UL,
        '7' => 0x7E060C1830303000UL,
        '8' => 0x3C66663C66663C00UL,
        '9' => 0x3C66663E060C3800UL,
        ':' => 0x0000181800181800UL,
        ';' => 0x0000181800181830UL,
        '<' => 0x060C1830180C0600UL,
        '=' => 0x00007E00007E0000UL,
        '>' => 0x6030180C18306000UL,
        '?' => 0x3C660C1818001800UL,
        '@' => 0x3C666E6E60603C00UL,
        // Uppercase - medieval/blackletter-inspired pixel font
        'A' => 0x183C66667E666600UL,
        'B' => 0x7C66667C66667C00UL,
        'C' => 0x3C66606060663C00UL,
        'D' => 0x786C66666C786000UL,
        'E' => 0x7E60607C60607E00UL,
        'F' => 0x7E60607C60606000UL,
        'G' => 0x3C66606E66663C00UL,
        'H' => 0x6666667E66666600UL,
        'I' => 0x3C18181818183C00UL,
        'J' => 0x0E06060606663C00UL,
        'K' => 0x666C7870786C6600UL,
        'L' => 0x6060606060607E00UL,
        'M' => 0x63776B636363630EUL,
        'N' => 0x6676665E66664600UL,
        'O' => 0x3C66666666663C00UL,
        'P' => 0x7C66667C60606000UL,
        'Q' => 0x3C6666666A6C3E00UL,
        'R' => 0x7C66667C6C666200UL,
        'S' => 0x3C66603C06663C00UL,
        'T' => 0x7E18181818181800UL,
        'U' => 0x6666666666663C00UL,
        'V' => 0x66666666663C1800UL,
        'W' => 0x636363636B7F3600UL,
        'X' => 0x66663C183C666600UL,
        'Y' => 0x6666663C18181800UL,
        'Z' => 0x7E060C1830607E00UL,
        '[' => 0x3C30303030303C00UL,
        '\\' => 0x4020100804020000UL,
        ']' => 0x3C0C0C0C0C0C3C00UL,
        '^' => 0x1836000000000000UL,
        '_' => 0x000000000000FF00UL,
        '`' => 0x3018000000000000UL,
        // Lowercase
        'a' => 0x0000386C447C4400UL,
        'b' => 0x60607C6666667C00UL,
        'c' => 0x00003C6060603C00UL,
        'd' => 0x06063E6666663E00UL,
        'e' => 0x00003C667E603C00UL,
        'f' => 0x1C30307C30303000UL,
        'g' => 0x00003E66663E063CUL,
        'h' => 0x60607C6666666600UL,
        'i' => 0x1800381818183C00UL,
        'j' => 0x0600060606063C00UL,
        'k' => 0x60606C7870786C00UL,
        'l' => 0x3818181818183C00UL,
        'm' => 0x0000667F6B636300UL,
        'n' => 0x00007C6666666600UL,
        'o' => 0x00003C6666663C00UL,
        'p' => 0x00007C66667C6060UL,
        'q' => 0x00003E66663E0606UL,
        'r' => 0x00007C6660606000UL,
        's' => 0x00003E603C067C00UL,
        't' => 0x30307C3030301C00UL,
        'u' => 0x0000666666663E00UL,
        'v' => 0x00006666663C1800UL,
        'w' => 0x0000636B6B7F3600UL,
        'x' => 0x0000663C183C6600UL,
        'y' => 0x00006666663E063CUL,
        'z' => 0x00007E0C18307E00UL,
        '{' => 0x0C18183018180C00UL,
        '|' => 0x1818181818181800UL,
        '}' => 0x30180C060C183000UL,
        '~' => 0x0000324C00000000UL,
        // German umlauts mapped to extended range won't fit in 32-126
        // but we support them via special rendering
        _ => 0x5AA5A55AA55A5AA5UL // checkerboard for unknown
    };

    public void Dispose()
    {
        _gl.DeleteTexture(TextureId);
    }
}
