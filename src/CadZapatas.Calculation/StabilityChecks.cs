namespace CadZapatas.Calculation;

/// <summary>
/// Comprobaciones de estabilidad global de una cimentacion superficial o muro:
/// deslizamiento, vuelco, subpresion. Referencia CTE DB-SE-C 4.3 y 4.4.
/// </summary>
public static class StabilityChecks
{
    /// <summary>
    /// Coeficiente de seguridad al deslizamiento CS = (V*tan(delta) + c_a * A + Ep) / H
    /// V: axil favorable, H: horizontal desfavorable, A: area de contacto.
    /// delta: angulo de rozamiento base-terreno (usualmente 2/3 phi).
    /// c_a: adherencia base-terreno (suelos cohesivos), usualmente 0.5*c.
    /// Ep: empuje pasivo movilizable (frecuentemente despreciado por seguridad).
    /// </summary>
    public static double SlidingSafetyFactor(double verticalN, double horizontalN,
                                              double deltaDeg, double adhesionPa, double areaM2,
                                              double passiveResistanceN = 0.0)
    {
        if (horizontalN <= 0) return double.PositiveInfinity;
        double resistance = verticalN * Math.Tan(deltaDeg * Math.PI / 180.0)
                          + adhesionPa * areaM2
                          + passiveResistanceN;
        return resistance / horizontalN;
    }

    /// <summary>
    /// Coeficiente de seguridad al vuelco CS = M_est / M_volc.
    /// M_est = sumatorio momentos estabilizantes respecto al borde critico.
    /// M_volc = sumatorio momentos volcadores respecto al mismo punto.
    /// </summary>
    public static double OverturningSafetyFactor(double stabilizingMomentNm, double overturningMomentNm)
    {
        if (overturningMomentNm <= 1e-9) return double.PositiveInfinity;
        return stabilizingMomentNm / overturningMomentNm;
    }

    /// <summary>
    /// Excentricidad maxima admisible en una zapata rigida. Para tensiones de contacto
    /// positivas en toda la superficie se requiere e &lt;= B/6 (nucleo central).
    /// Devuelve true si la resultante cae dentro del nucleo.
    /// </summary>
    public static bool ResultantInCore(double M, double N, double B)
    {
        if (N <= 0) return false;
        double e = Math.Abs(M / N);
        return e <= B / 6.0;
    }

    /// <summary>
    /// Tensiones maxima y minima de contacto en una zapata rigida rectangular
    /// bajo N axil y momentos Mx, My. Si hay separacion, se evalua zona comprimida triangular.
    /// Devuelve (qmax, qmin) en Pa. Si la resultante cae fuera del nucleo central,
    /// la distribucion es triangular y se reequilibra por Meyerhof.
    /// </summary>
    public static (double qMax, double qMin) ContactPressure(double N, double Mx, double My,
                                                              double B, double L)
    {
        double A = B * L;
        double ex = Mx / N;
        double ey = My / N;

        if (Math.Abs(ex) <= B / 6 && Math.Abs(ey) <= L / 6)
        {
            double Wx = L * B * B / 6.0;
            double Wy = B * L * L / 6.0;
            double q0 = N / A;
            double dqx = Math.Abs(Mx) / Wx;
            double dqy = Math.Abs(My) / Wy;
            double qMax = q0 + dqx + dqy;
            double qMin = q0 - dqx - dqy;
            return (qMax, qMin);
        }
        else
        {
            // Meyerhof simplificado: B' = B - 2*ex, L' = L - 2*ey, q = N / (B' * L')
            double bp = Math.Max(B - 2 * Math.Abs(ex), 0.1);
            double lp = Math.Max(L - 2 * Math.Abs(ey), 0.1);
            double q = N / (bp * lp);
            return (q, 0.0);
        }
    }

    /// <summary>
    /// Factor de seguridad a la subpresion = (peso propio + cargas verticales) / (empuje vertical del agua).
    /// Utilizado en cimentaciones bajo el nivel freatico o aliviaderos.
    /// </summary>
    public static double UpliftSafetyFactor(double totalVerticalWeightN, double upliftForceN)
    {
        if (upliftForceN <= 1e-9) return double.PositiveInfinity;
        return totalVerticalWeightN / upliftForceN;
    }
}
