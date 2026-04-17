using CadZapatas.Materials;

namespace CadZapatas.Reinforcement;

/// <summary>
/// Reglas normativas de armado segun Codigo Estructural RD 470/2021.
/// Arts. 32 a 36 (materiales), 55 (disposiciones), 58.4 (estribos), 60-62 (anclaje y solapes).
/// </summary>
public static class RebarRules
{
    /// <summary>
    /// Radio minimo de doblado del mandril (CE art. 34.6, tabla 34.6).
    /// Diametros ≤ 16: 4Ø para B400S/B500S, 5Ø para B500SD.
    /// Diametros > 16: 7Ø para B400S/B500S, 8Ø para B500SD.
    /// Devuelve el diametro del mandril en mm.
    /// </summary>
    public static double MinMandrelDiameterMm(int barDiameterMm, string steelGrade)
    {
        bool ductile = steelGrade == "B500SD";
        if (barDiameterMm <= 16)
            return ductile ? 5.0 * barDiameterMm : 4.0 * barDiameterMm;
        return ductile ? 8.0 * barDiameterMm : 7.0 * barDiameterMm;
    }

    /// <summary>
    /// Separacion libre minima entre barras paralelas (CE art. 55.1).
    /// max { Ø, 20 mm, (6/5) * dg } con dg tamano maximo del arido.
    /// </summary>
    public static double MinClearSpacingMm(int barDiameterMm, double aggregateMaxSizeMm = 20.0)
        => Math.Max(barDiameterMm, Math.Max(20.0, 1.2 * aggregateMaxSizeMm));

    /// <summary>
    /// Separacion maxima entre barras principales en elementos superficiales (CE tabla 55.2).
    /// Zapatas, losas: max 30 cm y 3 veces el canto.
    /// </summary>
    public static double MaxBarSpacingSlabMm(double slabThicknessM)
        => Math.Min(300.0, 3.0 * slabThicknessM * 1000.0);

    /// <summary>
    /// Cuantia geometrica minima de armadura de traccion en zapatas (CE tabla 55.2).
    /// 0.9 por mil para B400S, 0.9 por mil para B500S, 0.9 por mil para B500SD
    /// (armadura perpendicular a direccion de flexion en zapatas).
    /// NOTA: valor simplificado; la losa usa 1.8 por mil. SUPOSICION DE DISENO.
    /// </summary>
    public static double MinGeometricReinforcementRatioSlab(string steelGrade)
        => steelGrade switch
        {
            "B400S" => 0.0020,
            "B500S" => 0.0018,
            "B500SD" => 0.0018,
            "B500T" => 0.0018,
            _ => 0.0020
        };

    /// <summary>
    /// Longitud basica de anclaje por prolongacion recta (CE art. 60.2).
    /// lb = (Ø/4) * (σsd / fbd); para prediseño lb = m * Ø^2 con m tabulado (tabla 60.2).
    /// Aqui devolvemos en mm a partir de la formula racional.
    /// </summary>
    /// <param name="barDiameterMm">Diametro Ø en mm.</param>
    /// <param name="sigmaSdMPa">Tension en la barra en ELU (MPa). Usar fyd si desconocido.</param>
    /// <param name="fbdMPa">Tension de adherencia de calculo (MPa).</param>
    public static double BasicAnchorageLengthMm(int barDiameterMm, double sigmaSdMPa, double fbdMPa)
        => (barDiameterMm / 4.0) * (sigmaSdMPa / fbdMPa);

    /// <summary>
    /// Tension de adherencia de calculo fbd (CE art. 60.1.2).
    /// fbd = η1 * η2 * 2.25 * fctd; simplificado fbd = 2.25 * fctd para buenas condiciones y Ø ≤ 32.
    /// fctd = fctk,0.05 / γc = 0.7 * 0.3 * fck^(2/3) / 1.5
    /// </summary>
    public static double BondStressMPa(double fckMPa, bool goodBondConditions = true, int barDiameterMm = 16)
    {
        double fctk05 = 0.7 * 0.3 * Math.Pow(fckMPa, 2.0 / 3.0);
        double fctd = fctk05 / 1.5;
        double eta1 = goodBondConditions ? 1.0 : 0.7;
        double eta2 = barDiameterMm <= 32 ? 1.0 : (132.0 - barDiameterMm) / 100.0;
        return eta1 * eta2 * 2.25 * fctd;
    }

    /// <summary>
    /// Longitud neta de anclaje lbd = α * lb con α segun forma del gancho (CE 60.3).
    /// Devuelve lbd ≥ max(lb,min, 10Ø, 100 mm).
    /// </summary>
    public static double NetAnchorageLengthMm(double lbBasic, RebarHookType hook, int barDiameterMm,
                                               bool tension = true)
    {
        double alpha = hook switch
        {
            RebarHookType.None => 1.0,
            RebarHookType.Standard90 => 0.7,
            RebarHookType.Standard135 => 0.7,
            RebarHookType.Standard180 => 0.7,
            RebarHookType.SeismicHook135 => 0.7,
            _ => 1.0
        };
        double lbd = alpha * lbBasic;
        double lbMin = tension
            ? Math.Max(0.3 * lbBasic, Math.Max(10.0 * barDiameterMm, 100.0))
            : Math.Max(0.6 * lbBasic, Math.Max(10.0 * barDiameterMm, 100.0));
        return Math.Max(lbd, lbMin);
    }

    /// <summary>
    /// Longitud de solape l0 (CE art. 61). l0 = α1 * lbd con α1 = 1.0 para <25% barras solapadas,
    /// 1.4 si entre 25% y 50%, 2.0 para 100%. Aqui simplificado: alpha=1.4 (caso usual).
    /// </summary>
    public static double LapLengthMm(double lbd, double fractionLapped = 0.5)
    {
        double alpha1 = fractionLapped switch
        {
            <= 0.25 => 1.0,
            <= 0.50 => 1.4,
            <= 0.75 => 1.7,
            _ => 2.0
        };
        double l0min = Math.Max(0.3 * lbd, Math.Max(15.0 * 10.0, 200.0));  // 15Ø con Ø medio, simplificado
        return Math.Max(alpha1 * lbd, l0min);
    }

    /// <summary>
    /// Separacion maxima de estribos en vigas (CE art. 58.4.1 / 44.2.3.4.1).
    /// st,max = min (0.75 * d * (1 + cotα), 300 mm); con α=90 (cercos verticales): st,max = min(0.75d, 300).
    /// </summary>
    public static double MaxStirrupSpacingMm(double effectiveDepthM, double cotAlpha = 0.0)
        => Math.Min(300.0, 0.75 * effectiveDepthM * 1000.0 * (1.0 + cotAlpha));

    /// <summary>
    /// Cuantia minima de cortante ρw,min (CE art. 44.2.3.4.1).
    /// ρw,min = 0.02 * sqrt(fck/fyk). Devuelve fraccion adimensional.
    /// </summary>
    public static double MinShearReinforcementRatio(double fckMPa, double fykMPa)
        => 0.02 * Math.Sqrt(fckMPa / fykMPa);

    /// <summary>
    /// Verifica si el diametro y forma cumplen los requisitos constructivos basicos.
    /// Retorna lista de avisos (vacia si cumple).
    /// </summary>
    public static List<string> AuditBar(RebarBar bar, string concreteClass)
    {
        var issues = new List<string>();
        if (!RebarDiameters.Standard.Contains(bar.DiameterMm))
            issues.Add($"Diametro {bar.DiameterMm} mm no es normalizado (CE art. 33).");
        foreach (var bend in bar.Bends)
        {
            double minMandrel = MinMandrelDiameterMm(bar.DiameterMm, bar.SteelGrade);
            if (bend.InnerRadiusMm * 2.0 < minMandrel)
                issues.Add($"Radio de doblado {bend.InnerRadiusMm} mm inferior al mandril minimo " +
                           $"({minMandrel / 2.0} mm) CE art. 34.6.");
        }
        if (bar.DevelopedLengthM <= 0)
            issues.Add("Barra sin longitud definida.");
        return issues;
    }
}
