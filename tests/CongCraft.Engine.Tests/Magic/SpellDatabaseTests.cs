using CongCraft.Engine.Magic;

namespace CongCraft.Engine.Tests.Magic;

public class SpellDatabaseTests
{
    [Fact]
    public void Contains4Spells()
    {
        Assert.Equal(4, SpellDatabase.All.Count);
    }

    [Theory]
    [InlineData("fireball", SpellType.Projectile)]
    [InlineData("heal", SpellType.SelfHeal)]
    [InlineData("shield", SpellType.Shield)]
    [InlineData("ice_nova", SpellType.AreaEffect)]
    public void SpellExists(string id, SpellType expectedType)
    {
        var spell = SpellDatabase.Get(id);
        Assert.NotNull(spell);
        Assert.Equal(expectedType, spell.Type);
    }

    [Fact]
    public void AllSpellsHavePositiveManaCost()
    {
        foreach (var spell in SpellDatabase.All.Values)
        {
            Assert.True(spell.ManaCost > 0, $"{spell.Name} should have positive mana cost");
        }
    }

    [Fact]
    public void AllSpellsHavePositiveCooldown()
    {
        foreach (var spell in SpellDatabase.All.Values)
        {
            Assert.True(spell.Cooldown > 0, $"{spell.Name} should have positive cooldown");
        }
    }

    [Fact]
    public void GetSpellBar_Returns4Spells()
    {
        var bar = SpellDatabase.GetSpellBar();
        Assert.Equal(4, bar.Length);
    }

    [Fact]
    public void Fireball_HasDamage()
    {
        var fireball = SpellDatabase.Get("fireball")!;
        Assert.True(fireball.Damage > 0);
        Assert.True(fireball.Range > 0);
    }

    [Fact]
    public void Heal_HasHealAmount()
    {
        var heal = SpellDatabase.Get("heal")!;
        Assert.True(heal.HealAmount > 0);
    }

    [Fact]
    public void Shield_HasDuration()
    {
        var shield = SpellDatabase.Get("shield")!;
        Assert.True(shield.Duration > 0);
    }
}
