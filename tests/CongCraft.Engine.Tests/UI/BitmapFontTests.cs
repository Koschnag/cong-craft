namespace CongCraft.Engine.Tests.UI;

/// <summary>
/// Tests for bitmap font glyph data (not GL-dependent tests).
/// BitmapFont itself requires GL context, but we test the static data.
/// </summary>
public class BitmapFontTests
{
    [Fact]
    public void GlyphConstants_Valid()
    {
        Assert.Equal(16, CongCraft.Engine.UI.BitmapFont.GlyphWidth);
        Assert.Equal(16, CongCraft.Engine.UI.BitmapFont.GlyphHeight);
        Assert.Equal(32, CongCraft.Engine.UI.BitmapFont.FirstChar);
        Assert.Equal(126, CongCraft.Engine.UI.BitmapFont.LastChar);
        Assert.Equal(95, CongCraft.Engine.UI.BitmapFont.CharCount);
    }

    [Fact]
    public void AtlasDimensions_Valid()
    {
        Assert.Equal(16, CongCraft.Engine.UI.BitmapFont.AtlasCols);
        // 95 chars / 16 cols = 6 rows
        Assert.Equal(6, CongCraft.Engine.UI.BitmapFont.AtlasRows);
    }

    [Fact]
    public void TextRenderer_MeasureWidth_EmptyString()
    {
        float w = CongCraft.Engine.UI.TextRenderer.MeasureWidth("", 1.0f);
        Assert.Equal(0, w);
    }

    [Fact]
    public void TextRenderer_MeasureWidth_SingleChar()
    {
        float w = CongCraft.Engine.UI.TextRenderer.MeasureWidth("A", 1.0f);
        Assert.Equal(8, w); // 8px logical width * 1.0 scale
    }

    [Fact]
    public void TextRenderer_MeasureWidth_MultipleChars()
    {
        float w = CongCraft.Engine.UI.TextRenderer.MeasureWidth("Hello", 2.0f);
        Assert.Equal(5 * 8 * 2.0f, w); // 5 chars * 8px logical * 2.0 scale
    }

    [Fact]
    public void TextRenderer_MeasureWidth_MultilineUsesLongestLine()
    {
        float w = CongCraft.Engine.UI.TextRenderer.MeasureWidth("Hi\nHello", 1.0f);
        Assert.Equal(5 * 8 * 1.0f, w); // "Hello" is longest (5 chars)
    }
}
