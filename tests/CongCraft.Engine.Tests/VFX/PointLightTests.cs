using System.Numerics;
using CongCraft.Engine.VFX;

namespace CongCraft.Engine.Tests.VFX;

public class PointLightTests
{
    [Fact]
    public void PointLightData_DefaultEmpty()
    {
        var data = new PointLightData();
        Assert.Equal(0, data.Count);
    }

    [Fact]
    public void PointLightData_MaxLightsIsFour()
    {
        Assert.Equal(4, PointLightData.MaxLights);
    }

    [Fact]
    public void PointLightData_StoresLights()
    {
        var data = new PointLightData();
        data.Count = 2;
        data.Positions[0] = new Vector3(1, 2, 3);
        data.Colors[0] = new Vector3(1, 0.5f, 0);
        data.Intensities[0] = 1.5f;
        data.Radii[0] = 10f;

        Assert.Equal(new Vector3(1, 2, 3), data.Positions[0]);
        Assert.Equal(1.5f, data.Intensities[0]);
    }

    [Fact]
    public void PointLightComponent_Defaults()
    {
        var light = new PointLightComponent();
        Assert.True(light.Intensity > 0);
        Assert.True(light.Radius > 0);
        Assert.True(light.FlickerSpeed > 0);
    }
}
