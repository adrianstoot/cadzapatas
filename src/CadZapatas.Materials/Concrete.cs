namespace CadZapatas.Materials;

/// <summary>
/// Hormigon estructural o en masa. Valores caracteristicos y de calculo segun Codigo Estructural RD 470/2021.
/// fck en MPa (nominal), demas derivados por formulas normativas.
/// </summary>
public class ConcreteMaterial
{
    public string Designation { get; init; } = "HA-25";    // HA-25, HA-30, HM-20, HL-15...
    public ConcreteKind Kind { get; init; } = ConcreteKind.Structural;
    public double Fck_MPa { get; init; } = 25.0;
    public double FckCube_MPa { get; init; } = 30.0;
    public double Density_kgPerM3 { get; init; } = 2500.0;

    /// <summary>Resistencia media a compresion: fcm = fck + 8 MPa.</summary>
    public double Fcm_MPa => Fck_MPa + 8.0;

    /// <summary>Resistencia media a traccion: fctm (CE 40.4).</summary>
    public double Fctm_MPa => Fck_MPa <= 50
        ? 0.30 * Math.Pow(Fck_MPa, 2.0 / 3.0)
        : 2.12 * Math.Log(1 + Fcm_MPa / 10.0);

    /// <summary>Resistencia caracteristica inferior a traccion (5%).</summary>
    public double Fctk_005_MPa => 0.7 * Fctm_MPa;

    /// <summary>Resistencia caracteristica superior a traccion (95%).</summary>
    public double Fctk_095_MPa => 1.3 * Fctm_MPa;

    /// <summary>Modulo de deformacion longitudinal secante (CE 39.6): Ecm = 22 * (fcm/10)^0.3 GPa.</summary>
    public double Ecm_GPa => 22.0 * Math.Pow(Fcm_MPa / 10.0, 0.3);

    /// <summary>Resistencia de calculo: fcd = alpha_cc * fck / gamma_c.</summary>
    public double Fcd_MPa(double gammaC = 1.5, double alphaCc = 1.0) => alphaCc * Fck_MPa / gammaC;

    public string Consistency { get; set; } = "B";     // S: seca, P: plastica, B: blanda, F: fluida, L: liquida, AC: autocompactante
    public int MaxAggregate_mm { get; set; } = 20;
    public string CementType { get; set; } = "CEM I 42,5 R";

    /// <summary>Catalogo de hormigones estandar de uso habitual en cimentaciones espanolas.</summary>
    public static IReadOnlyList<ConcreteMaterial> Standard => new[]
    {
        new ConcreteMaterial { Designation = "HL-150/B/20", Kind = ConcreteKind.Lean,        Fck_MPa = 15, FckCube_MPa = 20 },
        new ConcreteMaterial { Designation = "HM-20/B/20",  Kind = ConcreteKind.Mass,        Fck_MPa = 20, FckCube_MPa = 25 },
        new ConcreteMaterial { Designation = "HA-25/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 25, FckCube_MPa = 30 },
        new ConcreteMaterial { Designation = "HA-30/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 30, FckCube_MPa = 37 },
        new ConcreteMaterial { Designation = "HA-35/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 35, FckCube_MPa = 45 },
        new ConcreteMaterial { Designation = "HA-40/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 40, FckCube_MPa = 50 },
        new ConcreteMaterial { Designation = "HA-45/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 45, FckCube_MPa = 55 },
        new ConcreteMaterial { Designation = "HA-50/B/20",  Kind = ConcreteKind.Structural,  Fck_MPa = 50, FckCube_MPa = 60 }
    };

    public static ConcreteMaterial ByDesignation(string des) =>
        Standard.FirstOrDefault(c => c.Designation.StartsWith(des, StringComparison.OrdinalIgnoreCase))
        ?? new ConcreteMaterial { Designation = des };
}

public enum ConcreteKind
{
    Lean,           // HL, limpieza
    Mass,           // HM, masa
    Structural,     // HA, armado
    Prestressed     // HP, pretensado
}
