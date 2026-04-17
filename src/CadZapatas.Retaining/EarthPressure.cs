namespace CadZapatas.Retaining;

/// <summary>
/// Teorias de empuje de tierras. Rankine y Coulomb con correcciones por cohesion,
/// inclinacion de trasdos, talud y rozamiento pared-terreno.
/// Referencia: CTE DB-SE-C seccion 4.6 (Contencion) y capitulo 6 para empujes.
/// </summary>
public static class EarthPressure
{
    private static double Deg2Rad(double d) => d * Math.PI / 180.0;

    /// <summary>
    /// Coeficiente de empuje en reposo: K0 = 1 - sin phi' (Jaky) para suelos normalmente consolidados.
    /// </summary>
    public static double K0(double phiDeg, double OCR = 1.0)
    {
        double phi = Deg2Rad(phiDeg);
        double k0nc = 1.0 - Math.Sin(phi);
        return k0nc * Math.Pow(OCR, Math.Sin(phi));
    }

    /// <summary>
    /// Coeficiente de empuje activo de Rankine (trasdos vertical, terreno horizontal).
    /// Ka = tan^2(45 - phi/2).
    /// </summary>
    public static double KaRankine(double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        return Math.Pow(Math.Tan(Math.PI / 4 - phi / 2), 2);
    }

    /// <summary>
    /// Coeficiente de empuje pasivo de Rankine.
    /// Kp = tan^2(45 + phi/2).
    /// </summary>
    public static double KpRankine(double phiDeg)
    {
        double phi = Deg2Rad(phiDeg);
        return Math.Pow(Math.Tan(Math.PI / 4 + phi / 2), 2);
    }

    /// <summary>
    /// Coeficiente activo de Coulomb con rozamiento pared-terreno delta, inclinacion de trasdos
    /// alpha (positivo si inclinado hacia el terreno) y talud del relleno beta.
    /// Referencia: CTE DB-SE-C 4.6 y formulacion clasica.
    /// </summary>
    public static double KaCoulomb(double phiDeg, double deltaDeg, double alphaDeg = 90, double betaDeg = 0)
    {
        double phi = Deg2Rad(phiDeg);
        double delta = Deg2Rad(deltaDeg);
        double alpha = Deg2Rad(alphaDeg);
        double beta = Deg2Rad(betaDeg);

        double num = Math.Pow(Math.Sin(alpha + phi), 2);
        double sqrtInner = 1 + Math.Sqrt(
            (Math.Sin(phi + delta) * Math.Sin(phi - beta)) /
            (Math.Sin(alpha - delta) * Math.Sin(alpha + beta)));
        double den = Math.Sin(alpha) * Math.Sin(alpha) * Math.Sin(alpha - delta) * Math.Pow(sqrtInner, 2);
        return num / den;
    }

    public static double KpCoulomb(double phiDeg, double deltaDeg, double alphaDeg = 90, double betaDeg = 0)
    {
        double phi = Deg2Rad(phiDeg);
        double delta = Deg2Rad(deltaDeg);
        double alpha = Deg2Rad(alphaDeg);
        double beta = Deg2Rad(betaDeg);

        double num = Math.Pow(Math.Sin(alpha - phi), 2);
        double sqrtInner = 1 - Math.Sqrt(
            (Math.Sin(phi + delta) * Math.Sin(phi + beta)) /
            (Math.Sin(alpha + delta) * Math.Sin(alpha + beta)));
        double den = Math.Sin(alpha) * Math.Sin(alpha) * Math.Sin(alpha + delta) * Math.Pow(sqrtInner, 2);
        return num / den;
    }

    /// <summary>
    /// Empuje activo triangular total sobre una altura H, con peso especifico gamma
    /// y coeficiente activo Ka. No incluye sobrecarga ni agua.
    /// E_a = 0.5 * Ka * gamma * H^2  [N/m de muro]
    /// </summary>
    public static double ActivePressureResultant(double kA, double gammaN_per_m3, double heightM)
        => 0.5 * kA * gammaN_per_m3 * heightM * heightM;

    /// <summary>
    /// Empuje por sobrecarga uniforme q: E_q = Ka * q * H   [N/m]
    /// </summary>
    public static double SurchargeResultant(double kA, double qPa, double heightM)
        => kA * qPa * heightM;

    /// <summary>
    /// Empuje hidrostatico: E_w = 0.5 * gamma_w * H_w^2    [N/m]. gamma_w = 9810 N/m3.
    /// </summary>
    public static double WaterResultant(double waterHeightM)
        => 0.5 * 9810.0 * waterHeightM * waterHeightM;

    /// <summary>
    /// Reduccion del empuje activo por cohesion (Rankine con c'):
    /// sigma_a(z) = Ka * gamma * z - 2 * c' * sqrt(Ka).
    /// Si la integral resulta negativa en la parte superior, se considera grieta de traccion.
    /// </summary>
    public static double ActiveResultantWithCohesion(double kA, double gammaN_per_m3, double cPa, double heightM)
    {
        double zCrack = 2 * cPa / (gammaN_per_m3 * Math.Sqrt(kA));
        if (zCrack >= heightM) return 0.0;  // suelo cohesivo sin empuje efectivo
        double effectiveH = heightM - zCrack;
        return 0.5 * kA * gammaN_per_m3 * effectiveH * effectiveH
             - 2 * cPa * Math.Sqrt(kA) * effectiveH + 2 * cPa * cPa / gammaN_per_m3;
    }
}
