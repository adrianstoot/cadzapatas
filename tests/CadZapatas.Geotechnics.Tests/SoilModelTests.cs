using CadZapatas.Geotechnics;
using Xunit;

namespace CadZapatas.Geotechnics.Tests;

public class SoilModelTests
{
    [Fact]
    public void LayerAtElevation_FindsCorrectStratum()
    {
        var m = new SoilModel { Name = "Test" };
        m.Layers.Add(new SoilLayer { Name = "A", TopElevation = 0, BottomElevation = -3 });
        m.Layers.Add(new SoilLayer { Name = "B", TopElevation = -3, BottomElevation = -10 });

        Assert.Equal("A", m.LayerAtElevation(-1)?.Name);
        Assert.Equal("B", m.LayerAtElevation(-5)?.Name);
    }

    [Fact]
    public void TrackedParameter_PrefersDesignValueWhenSet()
    {
        var p = new TrackedParameter { CharacteristicValue = 25, DesignValue = 20 };
        Assert.Equal(20, p.DesignValue);
    }

    [Fact]
    public void SubmergedUnitWeight_EqualsSatMinusWater()
    {
        var s = new SoilParameterSet
        {
            UnitWeightSaturated = TrackedParameter.Fixed(20000, "N/m3")
        };
        Assert.Equal(20000 - 9810, s.SubmergedUnitWeightN_Per_M3);
    }
}
