using CongCraft.Engine.Audio;

namespace CongCraft.Engine.Tests.Audio;

public class ProceduralMusicExtendedTests
{
    [Fact]
    public void GenerateMenuTheme_CorrectLength()
    {
        var samples = ProceduralMusic.GenerateMenuTheme(durationSeconds: 2);
        Assert.Equal(44100 * 2, samples.Length);
    }

    [Fact]
    public void GenerateMenuTheme_NotAllZeros()
    {
        var samples = ProceduralMusic.GenerateMenuTheme(durationSeconds: 2);
        Assert.True(samples.Any(s => s != 0), "Menu theme should not be silent");
    }

    [Fact]
    public void GenerateExplorationTheme_CorrectLength()
    {
        var samples = ProceduralMusic.GenerateExplorationTheme(durationSeconds: 3);
        Assert.Equal(44100 * 3, samples.Length);
    }

    [Fact]
    public void GenerateExplorationTheme_NotAllZeros()
    {
        var samples = ProceduralMusic.GenerateExplorationTheme(durationSeconds: 2);
        Assert.True(samples.Any(s => s != 0), "Exploration theme should not be silent");
    }

    [Fact]
    public void GenerateCombatTheme_CorrectLength()
    {
        var samples = ProceduralMusic.GenerateCombatTheme(durationSeconds: 2);
        Assert.Equal(44100 * 2, samples.Length);
    }

    [Fact]
    public void GenerateCombatTheme_NotAllZeros()
    {
        var samples = ProceduralMusic.GenerateCombatTheme(durationSeconds: 2);
        Assert.True(samples.Any(s => s != 0), "Combat theme should not be silent");
    }

    [Fact]
    public void GenerateCombatTheme_WithinRange()
    {
        var samples = ProceduralMusic.GenerateCombatTheme(durationSeconds: 2);
        foreach (var s in samples)
        {
            Assert.InRange(s, short.MinValue, short.MaxValue);
        }
    }

    [Fact]
    public void AllThemes_FadeIn()
    {
        var themes = new[]
        {
            ProceduralMusic.GenerateMenuTheme(5),
            ProceduralMusic.GenerateExplorationTheme(5),
            ProceduralMusic.GenerateCombatTheme(5)
        };

        foreach (var samples in themes)
        {
            double earlyAvg = samples.Take(100).Average(s => Math.Abs((double)s));
            double midAvg = samples.Skip(44100 * 2).Take(1000).Average(s => Math.Abs((double)s));
            Assert.True(earlyAvg < midAvg, "Themes should fade in");
        }
    }
}
