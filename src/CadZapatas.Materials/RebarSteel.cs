namespace CadZapatas.Materials;

/// <summary>
/// Acero para armaduras pasivas segun Codigo Estructural RD 470/2021.
/// </summary>
public class RebarSteelMaterial
{
    public string Grade { get; init; } = "B500SD";         // B400S, B500S, B500SD, B500T (mallazo)
    public double Fyk_MPa { get; init; } = 500.0;          // limite elastico caracteristico
    public double Ftk_MPa { get; init; } = 575.0;          // resistencia caracteristica a traccion
    public double Es_GPa { get; init; } = 200.0;           // modulo elastico
    public double Euk_Percent { get; init; } = 7.5;        // deformacion bajo carga maxima
    public string DuctilityClass { get; init; } = "C";     // A (baja), B (normal), C (alta)
    public bool Weldable { get; init; } = true;

    public double Fyd_MPa(double gammaS = 1.15) => Fyk_MPa / gammaS;

    public static IReadOnlyList<RebarSteelMaterial> Standard => new[]
    {
        new RebarSteelMaterial { Grade = "B400S",  Fyk_MPa = 400, Ftk_MPa = 440, Euk_Percent = 5.0, DuctilityClass = "B" },
        new RebarSteelMaterial { Grade = "B500S",  Fyk_MPa = 500, Ftk_MPa = 550, Euk_Percent = 5.0, DuctilityClass = "B" },
        new RebarSteelMaterial { Grade = "B500SD", Fyk_MPa = 500, Ftk_MPa = 575, Euk_Percent = 7.5, DuctilityClass = "C" },
        new RebarSteelMaterial { Grade = "B500T",  Fyk_MPa = 500, Ftk_MPa = 550, Euk_Percent = 2.5, DuctilityClass = "A" }
    };

    public static RebarSteelMaterial ByGrade(string grade) =>
        Standard.FirstOrDefault(s => s.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase))
        ?? new RebarSteelMaterial { Grade = grade };
}

/// <summary>
/// Diametros normalizados de barras corrugadas en Espana (mm).
/// </summary>
public static class RebarDiameters
{
    public static readonly int[] Standard = { 6, 8, 10, 12, 14, 16, 20, 25, 32, 40 };

    public static double AreaMm2(int diameter_mm) => Math.PI * diameter_mm * diameter_mm / 4.0;
    public static double WeightKgPerMeter(int diameter_mm) =>
        AreaMm2(diameter_mm) * 1e-6 * 7850.0;  // kg/m
}
