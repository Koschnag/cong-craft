using CongCraft.Engine.Audio;

namespace CongCraft.Engine.Tests.Audio;

public class ProceduralMusicTests
{
    [Fact]
    public void GenerateAmbientDrone_CorrectLength()
    {
        var samples = ProceduralMusic.GenerateAmbientDrone(sampleRate: 44100, durationSeconds: 2);
        Assert.Equal(44100 * 2, samples.Length);
    }

    [Fact]
    public void GenerateAmbientDrone_NotAllZeros()
    {
        var samples = ProceduralMusic.GenerateAmbientDrone(sampleRate: 44100, durationSeconds: 1);
        Assert.True(samples.Any(s => s != 0), "Audio samples should not all be zero");
    }

    [Fact]
    public void GenerateAmbientDrone_WithinRange()
    {
        var samples = ProceduralMusic.GenerateAmbientDrone(sampleRate: 44100, durationSeconds: 2);
        foreach (var s in samples)
        {
            Assert.InRange(s, short.MinValue, short.MaxValue);
        }
    }

    [Fact]
    public void GenerateAmbientDrone_FadesIn()
    {
        var samples = ProceduralMusic.GenerateAmbientDrone(sampleRate: 44100, durationSeconds: 5);
        // First 100 samples should be near zero (fade in)
        double earlyAvg = samples.Take(100).Average(s => Math.Abs((double)s));
        double midAvg = samples.Skip(44100 * 2).Take(1000).Average(s => Math.Abs((double)s));
        Assert.True(earlyAvg < midAvg, "Audio should fade in (early samples quieter)");
    }
}
