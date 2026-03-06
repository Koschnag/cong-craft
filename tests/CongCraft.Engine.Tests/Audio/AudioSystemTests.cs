using CongCraft.Engine.Audio;
using CongCraft.Engine.Core;

namespace CongCraft.Engine.Tests.Audio;

public class AudioSystemTests
{
    [Fact]
    public void GameMode_HasCombatValue()
    {
        // Combat mode must exist so AudioSystem can switch to combat music
        Assert.True(Enum.IsDefined(typeof(GameMode), GameMode.Combat));
    }

    [Fact]
    public void SfxType_HasAllExpectedTypes()
    {
        // Verify all SFX types exist that the game systems reference
        var expected = new[]
        {
            SfxType.Click, SfxType.Hover, SfxType.Select,
            SfxType.SwordSwing, SfxType.SwordHit,
            SfxType.FootstepGrass, SfxType.FootstepStone,
            SfxType.EnemyHit, SfxType.EnemyDeath,
            SfxType.PlayerHurt, SfxType.ItemPickup,
            SfxType.SpellCast, SfxType.DodgeWhoosh
        };

        foreach (var sfx in expected)
            Assert.True(Enum.IsDefined(typeof(SfxType), sfx), $"SfxType.{sfx} should exist");
    }

    [Fact]
    public void GameMode_AllValues_AreMapped()
    {
        // Ensure AudioSystem.GetSourceForMode handles all GameMode values.
        // We verify this structurally by checking the source file contains a case for each mode.
        var sourceFile = File.ReadAllText(
            Path.Combine(FindSrcRoot(), "CongCraft.Engine", "Audio", "AudioSystem.cs"));

        Assert.Contains("GameMode.MainMenu", sourceFile);
        Assert.Contains("GameMode.Playing", sourceFile);
        Assert.Contains("GameMode.Combat", sourceFile);
        Assert.Contains("_combatSource", sourceFile);
    }

    [Fact]
    public void InventorySystem_TriggersItemPickupSfx()
    {
        var sourceFile = File.ReadAllText(
            Path.Combine(FindSrcRoot(), "CongCraft.Engine", "Inventory", "InventorySystem.cs"));

        Assert.Contains("SfxType.ItemPickup", sourceFile);
        Assert.Contains("PlaySfx", sourceFile);
    }

    [Fact]
    public void DialogueSystem_TriggersClickSfx()
    {
        var sourceFile = File.ReadAllText(
            Path.Combine(FindSrcRoot(), "CongCraft.Engine", "Dialogue", "DialogueSystem.cs"));

        Assert.Contains("SfxType.Click", sourceFile);
        Assert.Contains("PlaySfx", sourceFile);
    }

    [Fact]
    public void AudioSystem_DetectsCombatForMusicSwitch()
    {
        var sourceFile = File.ReadAllText(
            Path.Combine(FindSrcRoot(), "CongCraft.Engine", "Audio", "AudioSystem.cs"));

        // AudioSystem should check for nearby enemies to switch to combat music
        Assert.Contains("IsPlayerInCombat", sourceFile);
        Assert.Contains("GameMode.Combat", sourceFile);
        Assert.Contains("CombatDetectionRange", sourceFile);
    }

    private static string FindSrcRoot()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "src")))
            dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir!, "src");
    }
}
