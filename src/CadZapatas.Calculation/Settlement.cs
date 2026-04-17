namespace CadZapatas.Calculation;

/// <summary>
/// Calculo de asientos. CTE DB-SE-C 4.3.4 y Anejo D.
/// Tres metodos implementados:
///   - Elastico (Steinbrenner / Schleicher) para cohesivos y empujes breves
///   - Edometrico (Terzaghi) para suelos cohesivos saturados consolidando
///   - Burland-Burbidge para arenas a partir de SPT
/// </summary>
public static class Settlement
{
    /// <summary>
    /// Asiento elastico en el centro de una zapata rigida rectangular:
    /// s = q * B * (1 - nu^2) * Is / E
    /// Is = factor de forma (adimensional): rectangular rigida 0.82 (Schleicher).
    /// </summary>
    public static double ElasticSettlementMeters(double qPa, double widthB_m, double E_Pa,
                                                  double nu, double formFactor = 0.82)
    {
        if (E_Pa <= 0) return double.PositiveInfinity;
        return qPa * widthB_m * (1 - nu * nu) * formFactor / E_Pa;
    }

    /// <summary>
    /// Asiento edometrico por consolidacion de una capa de espesor H bajo incremento de
    /// tension Δσ'. s = H * (Cc / (1+e0)) * log10((σ'_0 + Δσ') / σ'_0) para suelos normalmente
    /// consolidados; usa Cr si σ'_0 &lt; σ'_p (sobreconsolidado).
    /// </summary>
    public static double OedometerSettlementMeters(double layerThickness_m,
                                                    double effectiveStressBefore_Pa,
                                                    double effectiveStressIncrement_Pa,
                                                    double preconsolidationPressure_Pa,
                                                    double Cc, double Cr, double voidRatioE0)
    {
        double sigma0 = effectiveStressBefore_Pa;
        double sigmaF = sigma0 + effectiveStressIncrement_Pa;
        double sigmaP = Math.Max(preconsolidationPressure_Pa, sigma0);
        double s;
        if (sigmaF <= sigmaP)
        {
            s = layerThickness_m * (Cr / (1 + voidRatioE0)) * Math.Log10(sigmaF / sigma0);
        }
        else
        {
            double sPreConsol = sigma0 < sigmaP
                ? layerThickness_m * (Cr / (1 + voidRatioE0)) * Math.Log10(sigmaP / sigma0)
                : 0.0;
            double sNC = layerThickness_m * (Cc / (1 + voidRatioE0)) * Math.Log10(sigmaF / sigmaP);
            s = sPreConsol + sNC;
        }
        return Math.Max(s, 0.0);
    }

    /// <summary>
    /// Metodo de Burland-Burbidge para asientos en arenas a partir de SPT (N60).
    /// s = f_s * f_l * f_t * q' * B^0.7 * Ic  (mm, q' en kPa, B en m)
    /// Ic = 1.71 / N_avg^1.4  (asiento para arena normalmente consolidada).
    /// SUPOSICION DE DISENO: f_s = 1 (rect), f_l = 1 (corto plazo), f_t = 1 (estaticas).
    /// </summary>
    public static double BurlandBurbidgeMm(double qNetkPa, double B_m, double nAverage,
                                            double fs = 1.0, double fl = 1.0, double ft = 1.0)
    {
        if (nAverage <= 0) return 0.0;
        double ic = 1.71 / Math.Pow(nAverage, 1.4);
        return fs * fl * ft * qNetkPa * Math.Pow(B_m, 0.7) * ic;
    }

    /// <summary>
    /// Asiento diferencial admisible segun CTE DB-SE-C tabla 2.2:
    /// edificios corrientes 1/500; estructuras continuas 1/1000; estructuras con muros de fabrica 1/3000.
    /// Devuelve delta_s_max / L (adimensional) admisible.
    /// </summary>
    public static double MaxDifferentialRotation(StructureSensitivity sensitivity)
        => sensitivity switch
        {
            StructureSensitivity.HighSensitivity => 1.0 / 3000.0,   // muros de fabrica
            StructureSensitivity.Standard => 1.0 / 500.0,           // hormigon armado corriente
            StructureSensitivity.Flexible => 1.0 / 300.0,           // porticos metalicos flexibles
            _ => 1.0 / 500.0
        };
}

public enum StructureSensitivity
{
    HighSensitivity,        // muros de fabrica, acabados fragiles
    Standard,               // hormigon armado estandar
    Flexible                // estructura metalica flexible
}
