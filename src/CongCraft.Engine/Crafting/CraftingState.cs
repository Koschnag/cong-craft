using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Crafting;

/// <summary>
/// Singleton tracking the active crafting session state.
/// </summary>
public sealed class CraftingState : IComponent
{
    public bool IsOpen { get; set; }
    public CraftingStationType StationType { get; set; }
    public int SelectedRecipe { get; set; }
    public List<CraftingRecipe> AvailableRecipes { get; set; } = new();
}
