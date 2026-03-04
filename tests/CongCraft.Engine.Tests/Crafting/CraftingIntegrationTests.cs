using CongCraft.Engine.Crafting;
using CongCraft.Engine.Inventory;

namespace CongCraft.Engine.Tests.Crafting;

public class CraftingIntegrationTests
{
    [Fact]
    public void CanCraft_ReturnsTrueWhenAllIngredientsAvailable()
    {
        var inventory = new InventoryComponent();
        inventory.TryAdd(ItemDatabase.Get("iron_ore")!, 3);
        inventory.TryAdd(ItemDatabase.Get("gold_coin")!, 1);

        var recipe = CraftingDatabase.Get("craft_iron_sword")!;
        Assert.True(CraftingSystem.CanCraft(recipe, inventory));
    }

    [Fact]
    public void CanCraft_ReturnsFalseWhenMissingIngredient()
    {
        var inventory = new InventoryComponent();
        inventory.TryAdd(ItemDatabase.Get("iron_ore")!, 1); // Need 3

        var recipe = CraftingDatabase.Get("craft_iron_sword")!;
        Assert.False(CraftingSystem.CanCraft(recipe, inventory));
    }

    [Fact]
    public void CanCraft_ReturnsFalseWhenInventoryEmpty()
    {
        var inventory = new InventoryComponent();
        var recipe = CraftingDatabase.Get("craft_iron_sword")!;
        Assert.False(CraftingSystem.CanCraft(recipe, inventory));
    }

    [Fact]
    public void TryCraft_RemovesIngredientsAndAddsOutput()
    {
        var inventory = new InventoryComponent();
        inventory.TryAdd(ItemDatabase.Get("iron_ore")!, 5);
        inventory.TryAdd(ItemDatabase.Get("gold_coin")!, 2);

        var recipe = CraftingDatabase.Get("craft_iron_sword")!;
        bool result = CraftingSystem.TryCraft(recipe, inventory);

        Assert.True(result);
        Assert.Equal(2, inventory.CountOf("iron_ore"));   // 5 - 3
        Assert.Equal(1, inventory.CountOf("gold_coin"));   // 2 - 1
        Assert.Equal(1, inventory.CountOf("iron_sword"));  // Crafted
    }

    [Fact]
    public void TryCraft_FailsWhenMissingIngredients()
    {
        var inventory = new InventoryComponent();
        inventory.TryAdd(ItemDatabase.Get("iron_ore")!, 1);

        var recipe = CraftingDatabase.Get("craft_iron_sword")!;
        bool result = CraftingSystem.TryCraft(recipe, inventory);

        Assert.False(result);
        Assert.Equal(1, inventory.CountOf("iron_ore")); // Not consumed
    }

    [Fact]
    public void TryCraft_MultipleCraftsDepleteMaterials()
    {
        var inventory = new InventoryComponent();
        inventory.TryAdd(ItemDatabase.Get("herb")!, 4);
        inventory.TryAdd(ItemDatabase.Get("gold_coin")!, 2);

        var recipe = CraftingDatabase.Get("craft_health_potion")!;

        Assert.True(CraftingSystem.TryCraft(recipe, inventory));
        Assert.Equal(2, inventory.CountOf("herb"));
        Assert.Equal(1, inventory.CountOf("gold_coin"));
        Assert.Equal(1, inventory.CountOf("health_potion"));

        Assert.True(CraftingSystem.TryCraft(recipe, inventory));
        Assert.Equal(0, inventory.CountOf("herb"));
        Assert.Equal(0, inventory.CountOf("gold_coin"));
        Assert.Equal(2, inventory.CountOf("health_potion"));

        // Third craft should fail
        Assert.False(CraftingSystem.TryCraft(recipe, inventory));
    }

    [Theory]
    [InlineData("iron_ore")]
    [InlineData("herb")]
    [InlineData("leather")]
    [InlineData("wood")]
    [InlineData("cloth")]
    [InlineData("strength_elixir")]
    public void NewItemsExistInDatabase(string itemId)
    {
        Assert.NotNull(ItemDatabase.Get(itemId));
    }

    [Fact]
    public void CraftingState_DefaultValues()
    {
        var state = new CraftingState();
        Assert.False(state.IsOpen);
        Assert.Equal(0, state.SelectedRecipe);
        Assert.Empty(state.AvailableRecipes);
    }
}
