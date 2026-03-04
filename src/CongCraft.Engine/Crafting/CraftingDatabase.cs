namespace CongCraft.Engine.Crafting;

/// <summary>
/// Central registry of all crafting recipes, organized by station type.
/// </summary>
public static class CraftingDatabase
{
    private static readonly Dictionary<string, CraftingRecipe> Recipes = new();

    static CraftingDatabase()
    {
        // Anvil recipes (weapons and heavy armor)
        Register(new CraftingRecipe
        {
            Id = "craft_iron_sword", Name = "Forge Iron Sword",
            Station = CraftingStationType.Anvil,
            Ingredients = new List<CraftingIngredient>
            {
                new("iron_ore", 3),
                new("gold_coin", 1)
            },
            OutputItemId = "iron_sword"
        });
        Register(new CraftingRecipe
        {
            Id = "craft_chain_mail", Name = "Forge Chain Mail",
            Station = CraftingStationType.Anvil,
            Ingredients = new List<CraftingIngredient>
            {
                new("iron_ore", 5),
                new("leather", 2)
            },
            OutputItemId = "chain_mail"
        });
        Register(new CraftingRecipe
        {
            Id = "craft_iron_greaves", Name = "Forge Iron Greaves",
            Station = CraftingStationType.Anvil,
            Ingredients = new List<CraftingIngredient>
            {
                new("iron_ore", 3),
                new("leather", 1)
            },
            OutputItemId = "iron_greaves"
        });

        // Alchemy recipes (potions and elixirs)
        Register(new CraftingRecipe
        {
            Id = "craft_health_potion", Name = "Brew Health Potion",
            Station = CraftingStationType.Alchemy,
            Ingredients = new List<CraftingIngredient>
            {
                new("herb", 2),
                new("gold_coin", 1)
            },
            OutputItemId = "health_potion"
        });
        Register(new CraftingRecipe
        {
            Id = "craft_strength_elixir", Name = "Brew Strength Elixir",
            Station = CraftingStationType.Alchemy,
            Ingredients = new List<CraftingIngredient>
            {
                new("herb", 3),
                new("wolf_pelt", 1)
            },
            OutputItemId = "strength_elixir"
        });

        // Workbench recipes (light armor and tools)
        Register(new CraftingRecipe
        {
            Id = "craft_leather_helm", Name = "Craft Leather Helm",
            Station = CraftingStationType.Workbench,
            Ingredients = new List<CraftingIngredient>
            {
                new("leather", 3),
                new("gold_coin", 1)
            },
            OutputItemId = "leather_helm"
        });
        Register(new CraftingRecipe
        {
            Id = "craft_dark_blade", Name = "Forge Dark Blade",
            Station = CraftingStationType.Anvil,
            Ingredients = new List<CraftingIngredient>
            {
                new("iron_ore", 8),
                new("wolf_pelt", 2),
                new("gold_coin", 5)
            },
            OutputItemId = "dark_blade"
        });
    }

    private static void Register(CraftingRecipe recipe) => Recipes[recipe.Id] = recipe;

    public static CraftingRecipe? Get(string id) => Recipes.GetValueOrDefault(id);

    public static IReadOnlyDictionary<string, CraftingRecipe> All => Recipes;

    public static IEnumerable<CraftingRecipe> GetByStation(CraftingStationType station)
        => Recipes.Values.Where(r => r.Station == station);
}
