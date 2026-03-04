namespace CongCraft.Engine.Magic;

/// <summary>
/// Registry of all available spells.
/// </summary>
public static class SpellDatabase
{
    private static readonly Dictionary<string, SpellData> Spells = new();

    static SpellDatabase()
    {
        Register(new SpellData
        {
            Id = "fireball", Name = "Fireball",
            Type = SpellType.Projectile,
            ManaCost = 15f, Cooldown = 1.5f,
            Damage = 35f, Range = 12f,
            VfxR = 1f, VfxG = 0.4f, VfxB = 0.1f
        });
        Register(new SpellData
        {
            Id = "heal", Name = "Heal",
            Type = SpellType.SelfHeal,
            ManaCost = 20f, Cooldown = 3f,
            HealAmount = 40f,
            VfxR = 0.3f, VfxG = 1f, VfxB = 0.4f
        });
        Register(new SpellData
        {
            Id = "shield", Name = "Magic Shield",
            Type = SpellType.Shield,
            ManaCost = 12f, Cooldown = 5f,
            Duration = 4f,
            VfxR = 0.3f, VfxG = 0.5f, VfxB = 1f
        });
        Register(new SpellData
        {
            Id = "ice_nova", Name = "Ice Nova",
            Type = SpellType.AreaEffect,
            ManaCost = 25f, Cooldown = 4f,
            Damage = 20f, Range = 5f,
            VfxR = 0.6f, VfxG = 0.8f, VfxB = 1f
        });
    }

    private static void Register(SpellData spell) => Spells[spell.Id] = spell;

    public static SpellData? Get(string id) => Spells.GetValueOrDefault(id);

    public static IReadOnlyDictionary<string, SpellData> All => Spells;

    public static SpellData[] GetSpellBar() => new[]
    {
        Spells["fireball"],
        Spells["heal"],
        Spells["shield"],
        Spells["ice_nova"]
    };
}
