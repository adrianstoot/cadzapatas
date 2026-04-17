using CadZapatas.Core.Bim;

namespace CadZapatas.Geotechnics;

/// <summary>
/// Estrato / unidad geotecnica. Alberga parametros caracteristicos usados por el motor de calculo.
/// Cada parametro mantiene procedencia y trazabilidad (medido, caracteristico, de calculo).
/// </summary>
public class SoilLayer : BimObject
{
    public override string ObjectType => "SoilLayer";

    public SoilKind Kind { get; set; } = SoilKind.Sand;
    public string UscsClass { get; set; } = "SP";          // USCS: SP, SC, CL, ML, GW, etc.
    public string? Geology { get; set; }
    public string? Color { get; set; }
    public bool IsFill { get; set; }
    public ProblematicSoilFlag Problematic { get; set; } = ProblematicSoilFlag.None;

    // Geometria: definida por profundidad desde superficie o por elevacion absoluta.
    // En el modelo 1D (perfil vertical) es suficiente con top / bottom por sondeo.
    public double TopElevation { get; set; }
    public double BottomElevation { get; set; }
    public double Thickness => Math.Abs(TopElevation - BottomElevation);

    // Parametros caracteristicos (valores adoptados como representativos del estrato).
    public SoilParameterSet Parameters { get; set; } = new();
}

public enum SoilKind
{
    Clay, SiltyClay, Silt, SandySilt,
    Sand, SiltySand, ClayeySand,
    Gravel, SandyGravel,
    Rock, WeatheredRock,
    OrganicSoil, Peat,
    Fill
}

[Flags]
public enum ProblematicSoilFlag
{
    None        = 0,
    Expansive   = 1 << 0,
    Collapsible = 1 << 1,
    Soft        = 1 << 2,
    Organic     = 1 << 3,
    Karst       = 1 << 4,
    Swelling    = 1 << 5,
    Liquefiable = 1 << 6
}

/// <summary>
/// Conjunto de parametros geotecnicos con sus versiones caracteristica y de calculo.
/// Unidades SI: densidad en kg/m3, cohesion y presiones en Pa, angulos en radianes.
/// </summary>
public class SoilParameterSet
{
    public TrackedParameter UnitWeight { get; set; } = new();             // gamma (peso especifico natural) [N/m3]
    public TrackedParameter UnitWeightSaturated { get; set; } = new();    // gamma_sat [N/m3]
    public TrackedParameter CohesionEffective { get; set; } = new();      // c' [Pa]
    public TrackedParameter CohesionUndrained { get; set; } = new();      // c_u [Pa]
    public TrackedParameter FrictionAngleDegrees { get; set; } = new();   // phi' [deg]
    public TrackedParameter DeformationModulus { get; set; } = new();     // E [Pa]
    public TrackedParameter PoissonRatio { get; set; } = new();           // nu
    public TrackedParameter SptNCharacteristic { get; set; } = new();     // N60
    public TrackedParameter Permeability { get; set; } = new();           // k [m/s]
    public TrackedParameter SubgradeModulus { get; set; } = new();        // k_s [N/m3] coeficiente de balasto

    /// <summary>Peso especifico sumergido: gamma' = gamma_sat - gamma_water (agua = 9810 N/m3).</summary>
    public double SubmergedUnitWeightN_Per_M3 =>
        Math.Max(0, UnitWeightSaturated.CharacteristicValue - 9810.0);
}

/// <summary>
/// Parametro con trazabilidad: valor medido, caracteristico y de calculo.
/// </summary>
public class TrackedParameter
{
    public double? MeasuredValue { get; set; }            // valor bruto medido/ensayado
    public double CharacteristicValue { get; set; }       // valor caracteristico adoptado
    public double? DesignValue { get; set; }              // valor de calculo (tras aplicar gamma_M)
    public string? Source { get; set; }                   // "SPT en sondeo S-02 @ 4.5m", "correlacion N60", "estudio geotecnico pg.12"
    public string? StatisticalMethod { get; set; }        // "media - 1.645*s", "mediana", "cautious estimate"
    public string Unit { get; set; } = string.Empty;

    public static TrackedParameter Fixed(double value, string unit, string? source = null) => new()
    {
        CharacteristicValue = value,
        MeasuredValue = value,
        Unit = unit,
        Source = source
    };
}
