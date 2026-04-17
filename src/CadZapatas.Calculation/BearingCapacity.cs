namespace CadZapatas.Calculation;

/// <summary>
/// Formulacion de Brinch-Hansen para capacidad portante ultima.
/// Referencia: CTE DB-SE-C 4.3 (hundimiento). Incluye factores de forma, profundidad,
/// inclinacion de la carga, inclinacion de la base y talud.
/// Valida para cimentaciones superficiales con B/L ≤ 1 y ancho equivalente de Meyerhof B' = B-2e.
/// </summary>
public static class BearingCapacity
{
    private static double Deg2Rad(double d) => d * Math.PI / 180.0;

    /// <summary>
    /// Factor Nq = e^(pi*tan(phi)) * tan^2(45 + phi/2).
    /// </summary>
    public static double Nq(double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        return Math.Exp(Math.PI * Math.Tan(phi)) * Math.Pow(Math.Tan(Math.PI / 4 + phi / 2), 2);
    }

    /// <summary>Nc = (Nq - 1) * cot(phi); cuando phi=0 se usa Nc = 5.14.</summary>
    public static double Nc(double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        if (Math.Abs(phiDeg) < 1e-6) return 5.14;
        return (Nq(phiDeg) - 1.0) / Math.Tan(phi);
    }

    /// <summary>Ngamma = 2 * (Nq - 1) * tan(phi) (Vesic / Brinch-Hansen).</summary>
    public static double Ngamma(double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        return 2.0 * (Nq(phiDeg) - 1.0) * Math.Tan(phi);
    }

    /// <summary>Parametros geometricos efectivos de Meyerhof: B' = B - 2eB, L' = L - 2eL.</summary>
    public static (double bPrime, double lPrime) EffectiveDimensions(
        double B, double L, double eB = 0, double eL = 0)
        => (B - 2 * eB, L - 2 * eL);

    /// <summary>
    /// Factores de forma (Brinch-Hansen simplificado):
    /// sc = 1 + (B'/L') * (Nq/Nc);  sq = 1 + (B'/L') * tan(phi);  sg = 1 - 0.4 * (B'/L')
    /// </summary>
    public static (double sc, double sq, double sg) ShapeFactors(double bPrime, double lPrime, double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        double ratio = bPrime / lPrime;
        double sc = 1 + ratio * (Nq(phiDeg) / Math.Max(Nc(phiDeg), 1e-6));
        double sq = 1 + ratio * Math.Tan(phi);
        double sg = Math.Max(0.6, 1 - 0.4 * ratio);
        return (sc, sq, sg);
    }

    /// <summary>
    /// Factores de profundidad (Hansen): dq = 1 + 2*tan(phi)*(1-sin(phi))^2 * k
    /// k = D/B' si D/B' <= 1, k = arctan(D/B') en rad si D/B' > 1.
    /// </summary>
    public static (double dc, double dq, double dg) DepthFactors(double D, double bPrime, double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        double k = D / bPrime <= 1.0 ? D / bPrime : Math.Atan(D / bPrime);
        double dq = 1 + 2 * Math.Tan(phi) * Math.Pow(1 - Math.Sin(phi), 2) * k;
        double dc = phiDeg > 1e-6
            ? dq - (1 - dq) / (Nc(phiDeg) * Math.Tan(phi))
            : 1 + 0.4 * k;
        double dg = 1.0;
        return (dc, dq, dg);
    }

    /// <summary>
    /// Factores de inclinacion de la carga (H componente horizontal, V componente vertical, c cohesion, Af area efectiva).
    /// iq = (1 - H / (V + A'*c*cot(phi)))^m;   ig = (1 - H / (V + A'*c*cot(phi)))^(m+1)
    /// m = (2 + B'/L') / (1 + B'/L').
    /// </summary>
    public static (double ic, double iq, double ig) InclinationFactors(
        double H, double V, double cPa, double phiDeg, double bPrime, double lPrime)
    {
        double phi = Deg2Rad(phiDeg);
        double m = (2.0 + bPrime / lPrime) / (1.0 + bPrime / lPrime);
        double Af = bPrime * lPrime;
        double denom = V + Af * cPa * (phiDeg > 1e-6 ? (1.0 / Math.Tan(phi)) : 0.0);
        double argb = 1.0 - H / Math.Max(denom, 1e-6);
        argb = Math.Max(0.0, argb);
        double iq = Math.Pow(argb, m);
        double ig = Math.Pow(argb, m + 1);
        double ic = phiDeg > 1e-6
            ? iq - (1 - iq) / (Nc(phiDeg) * Math.Tan(phi))
            : 1 - (m * H) / (Af * cPa * 5.14);
        return (ic, iq, ig);
    }

    /// <summary>
    /// Presion de hundimiento ultima qu segun Brinch-Hansen en Pa (CTE DB-SE-C 4.3.3).
    /// qu = c * Nc * sc * dc * ic + q0 * Nq * sq * dq * iq + 0.5 * gamma * B' * Ngamma * sg * dg * ig
    /// </summary>
    public static double UltimatePressurePa(BearingCapacityInputs inp)
    {
        var (bp, lp) = EffectiveDimensions(inp.B, inp.L, inp.EccentricityB, inp.EccentricityL);
        double nc = Nc(inp.PhiDeg);
        double nq = Nq(inp.PhiDeg);
        double ng = Ngamma(inp.PhiDeg);
        var (sc, sq, sg) = ShapeFactors(bp, lp, inp.PhiDeg);
        var (dc, dq, dg) = DepthFactors(inp.EmbedmentDepth, bp, inp.PhiDeg);
        var (ic, iq, ig) = InclinationFactors(inp.HorizontalLoad, inp.VerticalLoad,
                                               inp.CohesionPa, inp.PhiDeg, bp, lp);
        double q0 = inp.OverburdenPressurePa;
        double qu =
              inp.CohesionPa * nc * sc * dc * ic
            + q0 * nq * sq * dq * iq
            + 0.5 * inp.EffectiveUnitWeight * bp * ng * sg * dg * ig;
        return Math.Max(qu, 0.0);
    }

    /// <summary>
    /// Presion admisible q_adm = qu / F con coeficiente global F (normalmente 3.0 en metodos de trabajo).
    /// En formato ELU el factor se aplica a acciones, no a qu: usar R_d = qu * A' y comparar con E_d.
    /// </summary>
    public static double AllowablePressurePa(double qu, double globalSafetyFactor = 3.0)
        => qu / globalSafetyFactor;
}

public class BearingCapacityInputs
{
    public double B { get; set; }                   // ancho [m]
    public double L { get; set; }                   // largo [m]
    public double EmbedmentDepth { get; set; }      // profundidad [m]
    public double EccentricityB { get; set; }
    public double EccentricityL { get; set; }

    public double PhiDeg { get; set; }
    public double CohesionPa { get; set; }
    public double EffectiveUnitWeight { get; set; }  // N/m3
    public double OverburdenPressurePa { get; set; }

    public double VerticalLoad { get; set; }         // N
    public double HorizontalLoad { get; set; }       // N
}
