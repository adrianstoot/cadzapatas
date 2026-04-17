using CadZapatas.Core.Audit;
using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;
using Xunit;

namespace CadZapatas.Core.Tests;

public class BimObjectTests
{
    [Fact]
    public void Project_DefaultsToCteAndCodigoEstructural()
    {
        var p = new Project { Name = "Test" };
        Assert.Equal("CTE_DB_SE_C_2019", p.GeotechnicalNormCode);
        Assert.Equal("CE_RD_470_2021", p.StructuralNormCode);
        Assert.False(p.EheCompatibilityMode);
    }

    [Fact]
    public void Point3D_DistanceTo_ReturnsEuclidean()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(3, 4, 0);
        Assert.Equal(5.0, a.DistanceTo(b), 6);
    }

    [Fact]
    public void CalcTrace_Unit_Aliases_Units()
    {
        var v = new CalcVariable { Symbol = "q", Value = 100, Unit = "kPa" };
        Assert.Equal("kPa", v.Units);
        v.Units = "MPa";
        Assert.Equal("MPa", v.Unit);
    }

    [Fact]
    public void CheckVerdictCode_HasError()
    {
        // La decision "Error" es importante para reglas que fallen en ejecucion.
        Assert.Contains(CheckVerdictCode.Error, Enum.GetValues<CheckVerdictCode>());
    }
}
