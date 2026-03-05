using CongCraft.Engine.Crafting;
using CongCraft.Engine.Inventory;

namespace CongCraft.Engine.Tests.Crafting;

public class CraftingRecipeTests
{
    [Fact]
    public void Recipe_HasValidProperties()
    {
        var recipe = new CraftingRecipe
        {
            Id = "test_recipe",
            Name = "Test Recipe",
            Station = CraftingStationType.Anvil,
            Ingredients = new List<CraftingIngredient> { new("iron_ore", 2) },
            OutputItemId = "iron_sword"
        };

        Assert.Equal("test_recipe", recipe.Id);
        Assert.Equal("Test Recipe", recipe.Name);
        Assert.Equal(CraftingStationType.Anvil, recipe.Station);
        Assert.Single(recipe.Ingredients);
        Assert.Equal(1, recipe.OutputCount); // Default
    }

    [Fact]
    public void CraftingIngredient_StoresItemIdAndCount()
    {
        var ingredient = new CraftingIngredient("herb", 3);
        Assert.Equal("herb", ingredient.ItemId);
        Assert.Equal(3, ingredient.Count);
    }

    [Fact]
    public void CraftingDatabase_ContainsExpectedRecipeCount()
    {
        Assert.Equal(11, CraftingDatabase.All.Count);
    }

    [Theory]
    [InlineData("craft_iron_sword", CraftingStationType.Anvil)]
    [InlineData("craft_chain_mail", CraftingStationType.Anvil)]
    [InlineData("craft_iron_greaves", CraftingStationType.Anvil)]
    [InlineData("craft_health_potion", CraftingStationType.Alchemy)]
    [InlineData("craft_strength_elixir", CraftingStationType.Alchemy)]
    [InlineData("craft_leather_helm", CraftingStationType.Workbench)]
    [InlineData("craft_dark_blade", CraftingStationType.Anvil)]
    public void CraftingDatabase_RecipeExists(string recipeId, CraftingStationType expectedStation)
    {
        var recipe = CraftingDatabase.Get(recipeId);
        Assert.NotNull(recipe);
        Assert.Equal(expectedStation, recipe.Station);
    }

    [Fact]
    public void CraftingDatabase_AllRecipesReferenceExistingItems()
    {
        foreach (var recipe in CraftingDatabase.All.Values)
        {
            // Output item must exist
            Assert.NotNull(ItemDatabase.Get(recipe.OutputItemId));

            // All ingredients must exist
            foreach (var ingredient in recipe.Ingredients)
            {
                Assert.NotNull(ItemDatabase.Get(ingredient.ItemId));
            }
        }
    }

    [Fact]
    public void CraftingDatabase_GetByStation_ReturnsCorrectRecipes()
    {
        var anvilRecipes = CraftingDatabase.GetByStation(CraftingStationType.Anvil).ToList();
        Assert.Equal(5, anvilRecipes.Count); // iron_sword, chain_mail, iron_greaves, dark_blade, bone_club
        Assert.All(anvilRecipes, r => Assert.Equal(CraftingStationType.Anvil, r.Station));
    }
}
