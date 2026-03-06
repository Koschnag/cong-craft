using CongCraft.Engine.Audio;

namespace CongCraft.Engine.Tests.Audio;

public class GameplaySfxTests
{
    [Theory]
    [InlineData(nameof(ProceduralMusic.GenerateSwordSwingSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateSwordHitSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateFootstepGrassSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateFootstepStoneSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateEnemyHitSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateEnemyDeathSfx))]
    [InlineData(nameof(ProceduralMusic.GeneratePlayerHurtSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateItemPickupSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateSpellCastSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateDodgeWhooshSfx))]
    public void GameplaySfx_NotEmpty_AndNotSilent(string methodName)
    {
        short[] data = methodName switch
        {
            nameof(ProceduralMusic.GenerateSwordSwingSfx) => ProceduralMusic.GenerateSwordSwingSfx(),
            nameof(ProceduralMusic.GenerateSwordHitSfx) => ProceduralMusic.GenerateSwordHitSfx(),
            nameof(ProceduralMusic.GenerateFootstepGrassSfx) => ProceduralMusic.GenerateFootstepGrassSfx(),
            nameof(ProceduralMusic.GenerateFootstepStoneSfx) => ProceduralMusic.GenerateFootstepStoneSfx(),
            nameof(ProceduralMusic.GenerateEnemyHitSfx) => ProceduralMusic.GenerateEnemyHitSfx(),
            nameof(ProceduralMusic.GenerateEnemyDeathSfx) => ProceduralMusic.GenerateEnemyDeathSfx(),
            nameof(ProceduralMusic.GeneratePlayerHurtSfx) => ProceduralMusic.GeneratePlayerHurtSfx(),
            nameof(ProceduralMusic.GenerateItemPickupSfx) => ProceduralMusic.GenerateItemPickupSfx(),
            nameof(ProceduralMusic.GenerateSpellCastSfx) => ProceduralMusic.GenerateSpellCastSfx(),
            nameof(ProceduralMusic.GenerateDodgeWhooshSfx) => ProceduralMusic.GenerateDodgeWhooshSfx(),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        Assert.True(data.Length > 0, $"{methodName} should generate samples");
        Assert.True(data.Any(s => s != 0), $"{methodName} should not be silent");
    }

    [Fact]
    public void GenerateAmbientWind_CorrectLength()
    {
        var data = ProceduralMusic.GenerateAmbientWind(2);
        Assert.Equal(44100 * 2, data.Length);
    }

    [Fact]
    public void GenerateAmbientWind_NotSilent()
    {
        var data = ProceduralMusic.GenerateAmbientWind(2);
        Assert.True(data.Any(s => s != 0), "Ambient wind should not be silent");
    }

    [Fact]
    public void AllSfx_WithinRange()
    {
        var allSfx = new[]
        {
            ProceduralMusic.GenerateSwordSwingSfx(),
            ProceduralMusic.GenerateSwordHitSfx(),
            ProceduralMusic.GenerateFootstepGrassSfx(),
            ProceduralMusic.GenerateFootstepStoneSfx(),
            ProceduralMusic.GenerateEnemyHitSfx(),
            ProceduralMusic.GenerateEnemyDeathSfx(),
            ProceduralMusic.GeneratePlayerHurtSfx(),
            ProceduralMusic.GenerateItemPickupSfx(),
            ProceduralMusic.GenerateSpellCastSfx(),
            ProceduralMusic.GenerateDodgeWhooshSfx(),
        };

        foreach (var sfx in allSfx)
        {
            foreach (var s in sfx)
            {
                Assert.InRange(s, short.MinValue, short.MaxValue);
            }
        }
    }
}
