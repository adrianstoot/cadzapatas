namespace CadZapatas.Calculation;

/// <summary>
/// Comprobaciones ELU/ELS de secciones de hormigon armado segun Codigo Estructural RD 470/2021.
/// Incluye flexion simple, cortante sin armadura, punzonamiento (zapatas y losas) y fisuracion.
/// </summary>
public static class ConcreteSectionChecks
{
    /// <summary>
    /// Calculo del area de armadura de traccion requerida por flexion simple,
    /// suponiendo rotura ductil (dominio 2/3, yd alcanzado). Metodo del parabola-rectangulo.
    /// M_d: momento de calculo (N*m), b: ancho (m), d: canto util (m),
    /// fck, fyk en Pa. Devuelve As en m2.
    /// </summary>
    public static double RequiredTensionAreaM2(double M_dNm, double bM, double dM,
                                                double fckPa, double fykPa)
    {
        double fcd = fckPa / 1.5;
        double fyd = fykPa / 1.15;
        double mu = M_dNm / (bM * dM * dM * fcd);
        double muLim = 0.295;      // para evitar compresion primaria; depende de acero
        if (mu > muLim) mu = muLim;     // SUPOSICION DE DISENO: requiere compresion adicional
        double omega = 1 - Math.Sqrt(1 - 2 * mu);
        double As = omega * bM * dM * fcd / fyd;
        return Math.Max(As, 0.0);
    }

    /// <summary>
    /// Cuantia minima geometrica de traccion por flexion (CE 55.2.2):
    /// As_min / (b*h) &gt;= 0.0018 para B500 (zapatas, losas). Retorna As_min [m2].
    /// </summary>
    public static double MinimumTensionAreaM2(double bM, double hM, string steelGrade = "B500SD")
    {
        double rho = steelGrade switch
        {
            "B400S" => 0.0020,
            "B500S" => 0.0018,
            "B500SD" => 0.0018,
            "B500T" => 0.0018,
            _ => 0.0020
        };
        return rho * bM * hM;
    }

    /// <summary>
    /// Resistencia a cortante sin armadura especifica Vu1 (CE art. 44.2.3.2.1):
    /// Vu1 = [ 0.18/γc * k * (100 * ρl * fck)^(1/3) ] * b * d
    /// con k = 1 + sqrt(200/d) &lt;= 2; ρl = As_l / (b*d) &lt;= 0.02, fck en MPa.
    /// Vu1 minimo = (0.075/γc) * k^1.5 * sqrt(fck) * b * d. Devuelve Vu1 en N.
    /// </summary>
    public static double ShearWithoutStirrupsN(double bM, double dM, double fckMPa,
                                                 double AsM2, double gammaC = 1.5)
    {
        double k = 1 + Math.Sqrt(0.2 / dM);
        if (k > 2) k = 2;
        double rho = Math.Min(AsM2 / (bM * dM), 0.02);
        double cRdc = 0.18 / gammaC;
        double vu1 = cRdc * k * Math.Pow(100 * rho * fckMPa, 1.0 / 3.0) * 1e6 * bM * dM;
        double vMin = (0.075 / gammaC) * Math.Pow(k, 1.5) * Math.Sqrt(fckMPa) * 1e6 * bM * dM;
        return Math.Max(vu1, vMin);
    }

    /// <summary>
    /// Comprobacion de punzonamiento en zapata aislada (CE 45.2).
    /// Perimetro critico u1 a distancia 2d del contorno del pilar.
    /// Tension maxima de punzonamiento tau_Rd = Vu1,max / (u1 * d) limitada a 0.5 * v * fcd con v = 0.6*(1-fck/250).
    /// </summary>
    public static double PunchingCapacityN(double columnPerimeterM, double d_M, double fckMPa,
                                             double gammaC = 1.5)
    {
        double fcd = fckMPa / gammaC * 1e6;
        double v = 0.6 * (1 - fckMPa / 250.0);
        double u1 = columnPerimeterM + 4 * Math.PI * d_M;       // aumentado por semicirculos en esquinas
        double tauMax = 0.5 * v * fcd;
        return tauMax * u1 * d_M;
    }

    /// <summary>
    /// Comprobacion simplificada de fisuracion (CE 49.2.3): separacion maxima de barras
    /// de traccion para w_k &lt; 0.3 mm en ambiente XC2/XC3.
    /// SUPOSICION DE DISENO: formulacion simplificada.
    /// </summary>
    public static double MaxBarSpacingForCrackingMm(double sigmaS_MPa)
    {
        // Tabla simplificada interpolada para wk = 0.3 mm
        if (sigmaS_MPa <= 160) return 300;
        if (sigmaS_MPa <= 200) return 250;
        if (sigmaS_MPa <= 240) return 200;
        if (sigmaS_MPa <= 280) return 150;
        if (sigmaS_MPa <= 320) return 100;
        return 50;
    }
}
