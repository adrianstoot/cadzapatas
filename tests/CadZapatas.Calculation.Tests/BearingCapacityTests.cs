using CadZapatas.Calculation;
using Xunit;

namespace CadZapatas.Calculation.Tests;

public class BearingCapacityTests
{
    [Fact]
    public void Nq_ForPhi30_IsAround18()
    {
        // Meyerhof/Hansen: Nq(30) ~ 18.4
        double nq = BearingCapacity.Nq(30);
        Assert.InRange(nq, 17.5, 19.0);
    }

    [Fact]
    public void Nc_ForPhi0_ReturnsPlusTwoPi()
    {
        // Prandtl: Nc(0) = 5.14
        Assert.Equal(5.14, BearingCapacity.Nc(0), 2);
    }

    [Fact]
    public void Ngamma_ForPhi35_IsPositive()
    {
        double ng = BearingCapacity.Ngamma(35);
        Assert.True(ng > 30 && ng < 50, $"Ngamma(35) fuera de rango esperado: {ng}");
    }

    [Fact]
    public void UltimatePressure_MatchesHandCalc_ForCohesiveSoil()
    {
        var inp = new BearingCapacityInputs
        {
            B = 2.0, L = 2.0, EmbedmentDepth = 1.0,
            PhiDeg = 0, CohesionPa = 50000,
            EffectiveUnitWeight = 18000,
            OverburdenPressurePa = 18000 * 1.0,
            VerticalLoad = 400000, HorizontalLoad = 0
        };
        double qu = BearingCapacity.UltimatePressurePa(inp);
        // Para suelo puramente cohesivo: qu = 5.14 * Su + q0 ~ 275 kPa
        Assert.InRange(qu / 1000, 250, 400);
    }
}
