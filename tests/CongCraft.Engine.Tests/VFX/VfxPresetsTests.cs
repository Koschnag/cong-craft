using CongCraft.Engine.VFX;

namespace CongCraft.Engine.Tests.VFX;

public class VfxPresetsTests
{
    [Fact]
    public void SwordSwing_HasReasonableConfig()
    {
        var config = VfxPresets.SwordSwing;
        Assert.True(config.MaxParticles > 0);
        Assert.True(config.MinLife > 0);
        Assert.True(config.MaxSpeed > config.MinSpeed);
    }

    [Fact]
    public void HitSparks_HasGravity()
    {
        var config = VfxPresets.HitSparks;
        Assert.True(config.Gravity.Y < 0, "Sparks should fall");
    }

    [Fact]
    public void BloodSplatter_IsRed()
    {
        var config = VfxPresets.BloodSplatter;
        Assert.True(config.StartColor.X > config.StartColor.Y, "Blood should be red-dominant");
        Assert.True(config.StartColor.X > config.StartColor.Z, "Blood should be red-dominant");
    }

    [Fact]
    public void TorchFire_RisesUpward()
    {
        var config = VfxPresets.TorchFire;
        Assert.True(config.Gravity.Y > 0, "Fire should rise");
    }

    [Fact]
    public void AllPresets_HavePositiveLifetime()
    {
        var presets = new[]
        {
            VfxPresets.SwordSwing, VfxPresets.HitSparks, VfxPresets.BloodSplatter,
            VfxPresets.TorchFire, VfxPresets.TorchEmbers
        };
        foreach (var p in presets)
        {
            Assert.True(p.MinLife > 0, "Min life should be positive");
            Assert.True(p.MaxLife >= p.MinLife, "Max life should >= min life");
        }
    }
}
