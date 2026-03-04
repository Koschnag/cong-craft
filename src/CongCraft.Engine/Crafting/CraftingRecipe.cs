namespace CongCraft.Engine.Crafting;

/// <summary>
/// Defines a crafting recipe with required ingredients, output, and station type.
/// </summary>
public sealed class CraftingRecipe
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required CraftingStationType Station { get; init; }
    public required List<CraftingIngredient> Ingredients { get; init; }
    public required string OutputItemId { get; init; }
    public int OutputCount { get; init; } = 1;
}

public readonly record struct CraftingIngredient(string ItemId, int Count);

public enum CraftingStationType
{
    Anvil,
    Alchemy,
    Workbench
}
