using CongCraft.Engine.Audio;

namespace CongCraft.Engine.Tests.Audio;

public class SfxGenerationTests
{
    [Theory]
    [InlineData(nameof(ProceduralMusic.GenerateClickSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateHoverSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateSelectSfx))]
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
    public void Sfx_NotEmpty_AndNotSilent(string methodName)
    {
        var samples = GenerateSfx(methodName);
        Assert.True(samples.Length > 0, $"{methodName} produced empty audio");
        Assert.True(samples.Any(s => s != 0), $"{methodName} is silent (all zeros)");
    }

    [Theory]
    [InlineData(nameof(ProceduralMusic.GenerateClickSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateHoverSfx))]
    [InlineData(nameof(ProceduralMusic.GenerateSelectSfx))]
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
    public void Sfx_ReasonableLength(string methodName)
    {
        var samples = GenerateSfx(methodName);
        // SFX should be short (less than 2 seconds at 44100 Hz)
        Assert.InRange(samples.Length, 100, 44100 * 2);
    }

    private static short[] GenerateSfx(string methodName) => methodName switch
    {
        nameof(ProceduralMusic.GenerateClickSfx) => ProceduralMusic.GenerateClickSfx(),
        nameof(ProceduralMusic.GenerateHoverSfx) => ProceduralMusic.GenerateHoverSfx(),
        nameof(ProceduralMusic.GenerateSelectSfx) => ProceduralMusic.GenerateSelectSfx(),
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
        _ => throw new ArgumentException($"Unknown SFX: {methodName}")
    };
}
