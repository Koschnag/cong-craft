using CongCraft.Engine.UI;

namespace CongCraft.Engine.Tests.UI;

public class TextRendererTests
{
    [Fact]
    public void WrapText_ShortText_NoWrap()
    {
        // "Hello" at scale 1.5 = 5 chars * 12px = 60px, well under 200px
        string result = TextRenderer.WrapText("Hello", 200f, 1.5f);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void WrapText_LongText_WrapsAtWordBoundary()
    {
        // maxWidth = 120px, charW = 12px (scale 1.5), so 10 chars per line
        string result = TextRenderer.WrapText("Hello world test", 120f, 1.5f);
        Assert.Contains("\n", result);
        // Each line should be <= 10 chars
        foreach (string line in result.Split('\n'))
            Assert.True(line.Length <= 10, $"Line '{line}' exceeds max width ({line.Length} chars)");
    }

    [Fact]
    public void WrapText_DialogueText_FitsInPanel()
    {
        // Real dialogue: panel is 620px wide with 30px padding = 590px usable
        float panelTextWidth = 590f;
        float scale = 1.5f;
        float charW = 8 * scale; // BitmapFont.LogicalWidth * scale
        int maxCharsPerLine = (int)(panelTextWidth / charW);

        string dialogue = "Iron is scarce since the mines fell to darkness. Bring me wolf pelts and I can trade you a fine blade.";
        string wrapped = TextRenderer.WrapText(dialogue, panelTextWidth, scale);

        foreach (string line in wrapped.Split('\n'))
        {
            Assert.True(line.Length <= maxCharsPerLine,
                $"Line '{line}' ({line.Length} chars) exceeds panel width ({maxCharsPerLine} chars max)");
        }
    }

    [Fact]
    public void WrapText_VeryLongDialogue_AllLinesFitPanel()
    {
        float panelTextWidth = 590f;
        float scale = 1.5f;
        float charW = 8 * scale;
        int maxCharsPerLine = (int)(panelTextWidth / charW);

        string dialogue = "Bandits on the north road, wolves in the eastern forest, and strange lights from the mountains at night. Business has never been worse... or better, depending on how you look at it.";
        string wrapped = TextRenderer.WrapText(dialogue, panelTextWidth, scale);

        foreach (string line in wrapped.Split('\n'))
        {
            Assert.True(line.Length <= maxCharsPerLine,
                $"Line '{line}' ({line.Length} chars) exceeds panel width ({maxCharsPerLine} chars max)");
        }

        // Should still contain all the original words
        Assert.Equal(dialogue, wrapped.Replace('\n', ' '));
    }

    [Fact]
    public void WrapText_EmptyOrNull_ReturnsAsIs()
    {
        Assert.Equal("", TextRenderer.WrapText("", 200f, 1.5f));
        Assert.Null(TextRenderer.WrapText(null!, 200f, 1.5f));
    }

    [Fact]
    public void WrapText_PreservesExistingNewlines()
    {
        string text = "Line one\nLine two";
        string result = TextRenderer.WrapText(text, 500f, 1.5f);
        Assert.Contains("\n", result);
    }

    [Fact]
    public void MeasureWidth_SingleLine()
    {
        float width = TextRenderer.MeasureWidth("Hello", 1.5f);
        Assert.Equal(5 * 8 * 1.5f, width);
    }

    [Fact]
    public void MeasureWidth_MultiLine_ReturnsLongest()
    {
        float width = TextRenderer.MeasureWidth("Hi\nHello", 1.5f);
        Assert.Equal(5 * 8 * 1.5f, width); // "Hello" is longest
    }
}
