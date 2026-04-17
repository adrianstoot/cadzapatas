namespace CadZapatas.Materials;

/// <summary>
/// Clases de exposicion segun Codigo Estructural. Relevante para durabilidad,
/// recubrimiento minimo y seleccion de hormigon.
/// </summary>
public class ExposureClass
{
    public string Code { get; init; } = "XC2";
    public string Description { get; init; } = string.Empty;
    public double MinFck_MPa { get; init; } = 25;
    public double MaxWaterCementRatio { get; init; } = 0.65;
    public double MinCementContent_kgPerM3 { get; init; } = 275;
    /// <summary>c_min,dur (mm) para clase estructural S4 y 50 anos de vida util.</summary>
    public double MinCoverDurability_mm_S4 { get; init; } = 25;

    public static readonly ExposureClass X0  = new() { Code = "X0",  Description = "Sin riesgo de corrosion o ataque",               MinFck_MPa = 15, MaxWaterCementRatio = 0.75, MinCementContent_kgPerM3 = 200, MinCoverDurability_mm_S4 = 15 };
    public static readonly ExposureClass XC1 = new() { Code = "XC1", Description = "Carbonatacion: seco o permanentemente humedo",    MinFck_MPa = 25, MaxWaterCementRatio = 0.65, MinCementContent_kgPerM3 = 275, MinCoverDurability_mm_S4 = 20 };
    public static readonly ExposureClass XC2 = new() { Code = "XC2", Description = "Carbonatacion: humedo, raramente seco",           MinFck_MPa = 25, MaxWaterCementRatio = 0.60, MinCementContent_kgPerM3 = 275, MinCoverDurability_mm_S4 = 30 };
    public static readonly ExposureClass XC3 = new() { Code = "XC3", Description = "Carbonatacion: humedad moderada",                  MinFck_MPa = 30, MaxWaterCementRatio = 0.55, MinCementContent_kgPerM3 = 280, MinCoverDurability_mm_S4 = 30 };
    public static readonly ExposureClass XC4 = new() { Code = "XC4", Description = "Carbonatacion: ciclos humedo-seco",                MinFck_MPa = 30, MaxWaterCementRatio = 0.50, MinCementContent_kgPerM3 = 300, MinCoverDurability_mm_S4 = 35 };
    public static readonly ExposureClass XD1 = new() { Code = "XD1", Description = "Cloruros no marinos: humedad moderada",            MinFck_MPa = 30, MaxWaterCementRatio = 0.55, MinCementContent_kgPerM3 = 300, MinCoverDurability_mm_S4 = 40 };
    public static readonly ExposureClass XD2 = new() { Code = "XD2", Description = "Cloruros no marinos: humedo, raramente seco",      MinFck_MPa = 30, MaxWaterCementRatio = 0.55, MinCementContent_kgPerM3 = 300, MinCoverDurability_mm_S4 = 40 };
    public static readonly ExposureClass XD3 = new() { Code = "XD3", Description = "Cloruros no marinos: ciclos humedo-seco",          MinFck_MPa = 35, MaxWaterCementRatio = 0.45, MinCementContent_kgPerM3 = 320, MinCoverDurability_mm_S4 = 45 };
    public static readonly ExposureClass XS1 = new() { Code = "XS1", Description = "Cloruros marinos: aire con sales",                  MinFck_MPa = 30, MaxWaterCementRatio = 0.50, MinCementContent_kgPerM3 = 300, MinCoverDurability_mm_S4 = 40 };
    public static readonly ExposureClass XS2 = new() { Code = "XS2", Description = "Cloruros marinos: sumergido",                       MinFck_MPa = 35, MaxWaterCementRatio = 0.45, MinCementContent_kgPerM3 = 320, MinCoverDurability_mm_S4 = 40 };
    public static readonly ExposureClass XS3 = new() { Code = "XS3", Description = "Cloruros marinos: zona de mareas",                  MinFck_MPa = 35, MaxWaterCementRatio = 0.45, MinCementContent_kgPerM3 = 340, MinCoverDurability_mm_S4 = 45 };
    public static readonly ExposureClass XA1 = new() { Code = "XA1", Description = "Ataque quimico debil",                              MinFck_MPa = 30, MaxWaterCementRatio = 0.55, MinCementContent_kgPerM3 = 300, MinCoverDurability_mm_S4 = 30 };
    public static readonly ExposureClass XA2 = new() { Code = "XA2", Description = "Ataque quimico moderado",                           MinFck_MPa = 30, MaxWaterCementRatio = 0.50, MinCementContent_kgPerM3 = 320, MinCoverDurability_mm_S4 = 35 };
    public static readonly ExposureClass XA3 = new() { Code = "XA3", Description = "Ataque quimico fuerte",                             MinFck_MPa = 35, MaxWaterCementRatio = 0.45, MinCementContent_kgPerM3 = 360, MinCoverDurability_mm_S4 = 40 };

    public static readonly IReadOnlyList<ExposureClass> All = new[] { X0, XC1, XC2, XC3, XC4, XD1, XD2, XD3, XS1, XS2, XS3, XA1, XA2, XA3 };

    public static ExposureClass ByCode(string code)
        => All.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) ?? XC2;
}

/// <summary>
/// Calculadora de recubrimiento nominal segun Codigo Estructural.
/// c_nom = c_min + Delta_c_dev, con c_min = max(c_min,b; c_min,dur; 10mm).
/// </summary>
public static class CoverCalculator
{
    public const double DefaultExecutionTolerance_mm = 10;

    public static double MinCover_mm(
        int barDiameter_mm,
        ExposureClass exposure,
        int maxAggregate_mm = 20,
        string structuralClass = "S4")
    {
        // c_min,b por adherencia: igual a diametro; si agregado > 32, c_min,b = diametro + 5mm
        double cMinBond = barDiameter_mm + (maxAggregate_mm > 32 ? 5 : 0);
        double cMinDur = exposure.MinCoverDurability_mm_S4;
        // ajuste por clase estructural (simplificado)
        double adj = structuralClass switch
        {
            "S1" => -10,
            "S2" => -5,
            "S3" => -5,
            "S4" => 0,
            "S5" => 5,
            "S6" => 10,
            _ => 0
        };
        cMinDur = Math.Max(10, cMinDur + adj);
        return Math.Max(10, Math.Max(cMinBond, cMinDur));
    }

    public static double NominalCover_mm(
        int barDiameter_mm,
        ExposureClass exposure,
        int maxAggregate_mm = 20,
        double executionTolerance_mm = DefaultExecutionTolerance_mm,
        string structuralClass = "S4")
    {
        return MinCover_mm(barDiameter_mm, exposure, maxAggregate_mm, structuralClass) + executionTolerance_mm;
    }
}
