using CongCraft.Engine.Crafting;

namespace CongCraft.Engine.Tests.Crafting;

public class CraftingComponentTests
{
    [Fact]
    public void CraftingComponent_DefaultInteractionRadius()
    {
        var component = new CraftingComponent { StationType = CraftingStationType.Anvil };
        Assert.Equal(3f, component.InteractionRadius);
    }

    [Fact]
    public void CraftingComponent_StoresStationType()
    {
        var component = new CraftingComponent { StationType = CraftingStationType.Alchemy };
        Assert.Equal(CraftingStationType.Alchemy, component.StationType);
    }

    [Fact]
    public void CraftingComponent_CustomRadius()
    {
        var component = new CraftingComponent
        {
            StationType = CraftingStationType.Workbench,
            InteractionRadius = 5f
        };
        Assert.Equal(5f, component.InteractionRadius);
    }
}
