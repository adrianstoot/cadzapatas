using CadZapatas.Core.Primitives;

namespace CadZapatas.Reinforcement;

/// <summary>
/// Barra de armadura individual. Representa una barra de acero corrugado (redondo)
/// con diametro normalizado, longitud, forma (shape code) y plegados.
/// Referencia: Codigo Estructural RD 470/2021 arts. 33, 34 y Anejo 11.
/// </summary>
public class RebarBar
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Marca de despiece (ej. "A01", "P3", etc.).</summary>
    public string Mark { get; set; } = string.Empty;

    /// <summary>Diametro nominal en mm (segun RebarDiameters.Standard).</summary>
    public int DiameterMm { get; set; } = 12;

    /// <summary>Designacion del acero (B400S, B500S, B500SD, B500T).</summary>
    public string SteelGrade { get; set; } = "B500SD";

    /// <summary>Longitud total desarrollada (recta) en metros, excluidos solapes.</summary>
    public double DevelopedLengthM { get; set; }

    /// <summary>Numero de barras identicas a esta.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Codigo de forma segun despiece (BS 8666 / tabla EHE Anejo 11).</summary>
    public string ShapeCode { get; set; } = "00";    // 00 = recta

    /// <summary>Lista de puntos que definen el trazado 3D de la barra (centroide).</summary>
    public List<Point3D> Path { get; set; } = new();

    /// <summary>Plegados con su angulo y radio de curvatura (en mm).</summary>
    public List<RebarBend> Bends { get; set; } = new();

    /// <summary>Gancho en el extremo inicial (ver RebarHookType).</summary>
    public RebarHookType StartHook { get; set; } = RebarHookType.None;

    /// <summary>Gancho en el extremo final.</summary>
    public RebarHookType EndHook { get; set; } = RebarHookType.None;

    /// <summary>Recubrimiento nominal al que se ha disenado la barra (mm).</summary>
    public double NominalCoverMm { get; set; } = 35.0;

    /// <summary>Funcion resistente (traccion principal, armadura de piel, transversal, etc.).</summary>
    public RebarFunction Function { get; set; } = RebarFunction.MainTension;

    /// <summary>Elemento estructural al que pertenece.</summary>
    public Guid OwnerElementId { get; set; }

    /// <summary>Seccion transversal de la barra en mm^2 (Ø^2*pi/4).</summary>
    public double AreaMm2 => Math.PI * DiameterMm * DiameterMm / 4.0;

    /// <summary>Peso por metro (kg/m) = 0.006165 * Ø^2, con Ø en mm (densidad 7850 kg/m3).</summary>
    public double UnitWeightKgPerMeter => 0.00617 * DiameterMm * DiameterMm;

    /// <summary>Peso total de todas las barras identicas (kg).</summary>
    public double TotalWeightKg => DevelopedLengthM * Quantity * UnitWeightKgPerMeter;
}

/// <summary>
/// Plegado de una barra con angulo (grados) y radio de curvatura interno (mm).
/// Codigo Estructural art. 34.6 y Anejo 11 definen radios minimos segun Ø y acero.
/// </summary>
public class RebarBend
{
    public double AngleDegrees { get; set; }
    public double InnerRadiusMm { get; set; }
    public int PathSegmentIndex { get; set; }       // en que tramo del Path esta el plegado
}

/// <summary>
/// Tipo de gancho extremo (Codigo Estructural Anejo 11).
/// </summary>
public enum RebarHookType
{
    None,
    Standard90,         // patilla recta
    Standard135,        // 135 grados (usual en estribos)
    Standard180,        // gancho completo (semicircular)
    SeismicHook135      // 135 grados con prolongacion minima 10Ø (sismico)
}

/// <summary>
/// Funcion que desempena la barra en el elemento.
/// </summary>
public enum RebarFunction
{
    MainTension,            // armadura principal de traccion
    MainCompression,        // armadura principal de compresion
    Distribution,           // armadura de reparto
    Skin,                   // armadura de piel (vigas cantos > 1 m)
    Shear,                  // cortante / estribo cerrado
    TorsionLongitudinal,
    TorsionTransverse,
    Punching,               // punzonamiento
    Anchorage,              // patilla / gancho
    ConnectionStarter,      // esperas / arranques
    TemperatureShrinkage    // retraccion / temperatura
}
