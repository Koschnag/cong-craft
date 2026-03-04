using System.Numerics;
using CongCraft.Engine.UI;

namespace CongCraft.Engine.Tests.UI;

public class MinimapTests
{
    [Fact]
    public void MinimapData_DefaultValues()
    {
        var data = new MinimapData();
        Assert.Equal(32, MinimapData.Resolution);
        Assert.Equal(60f, MinimapData.WorldRange);
        Assert.Equal(150f, MinimapData.MapSize);
        Assert.Equal(32 * 32, data.TerrainColors.Length);
        Assert.Empty(data.Markers);
    }

    [Fact]
    public void MinimapData_CellSize()
    {
        var data = new MinimapData();
        Assert.Equal(150f / 32f, data.CellSize, 0.01f);
    }

    [Fact]
    public void MinimapData_WorldCellSize()
    {
        var data = new MinimapData();
        Assert.Equal(120f / 32f, data.WorldCellSize, 0.01f);
    }

    [Fact]
    public void MinimapMarker_StoresValues()
    {
        var marker = new MinimapMarker
        {
            ScreenPos = new Vector2(100, 200),
            Color = new Vector4(1, 0, 0, 1),
            Size = 5f
        };

        Assert.Equal(100f, marker.ScreenPos.X);
        Assert.Equal(1f, marker.Color.X);
        Assert.Equal(5f, marker.Size);
    }

    [Fact]
    public void MinimapData_MarkersCanBeAdded()
    {
        var data = new MinimapData();
        data.Markers.Add(new MinimapMarker
        {
            ScreenPos = new Vector2(50, 50),
            Color = new Vector4(0, 1, 0, 1),
            Size = 4f
        });

        Assert.Single(data.Markers);
    }

    [Fact]
    public void MinimapData_TerrainColorsInitializedToDefault()
    {
        var data = new MinimapData();
        Assert.All(data.TerrainColors, c => Assert.Equal(Vector4.Zero, c));
    }
}
