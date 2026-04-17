using CadZapatas.Core.Primitives;

namespace CadZapatas.Core.Bim;

/// <summary>
/// Caso de carga. Agrupa cargas del mismo tipo y categoria para combinarlas segun norma.
/// </summary>
public class LoadCase : BimObject
{
    public override string ObjectType => "LoadCase";
    public LoadType Type { get; set; } = LoadType.Permanent;
    public LoadCategory Category { get; set; } = LoadCategory.G_SelfWeight;
    public string? CombinationFactorSet { get; set; }   // ej: "A" (imposed), "H" (snow)...
}

public enum LoadType
{
    Permanent,          // G
    Variable,           // Q
    Accidental,         // A
    Seismic,            // AE
    Prestress           // P
}

public enum LoadCategory
{
    G_SelfWeight,           // Peso propio
    G_DeadLoad,             // Cargas muertas permanentes
    G_EarthPressure,        // Empuje de tierras
    G_WaterPressure,        // Empuje hidrostatico
    Q_ImposedA,             // Sobrecarga de uso A (viviendas, zonas residenciales)
    Q_ImposedB,             // Oficinas
    Q_ImposedC,             // Zonas de acceso al publico
    Q_ImposedD,             // Zonas comerciales
    Q_ImposedE,             // Zonas de almacen
    Q_ImposedF,             // Cubiertas transitables
    Q_ImposedG,             // Cubiertas accesibles para mantenimiento
    Q_ImposedH,             // Cubiertas no transitables
    Q_Snow,
    Q_Wind,
    Q_Temperature,
    A_Impact,
    A_Fire,
    AE_Seismic
}

/// <summary>
/// Carga aplicada a un elemento. Puede ser puntual, distribuida lineal, distribuida por area o momento.
/// </summary>
public class Load : BimObject
{
    public override string ObjectType => "Load";
    public Guid LoadCaseId { get; set; }
    public Guid AppliedToId { get; set; }   // id del elemento al que se aplica
    public LoadApplicationKind Kind { get; set; } = LoadApplicationKind.Point;

    public Point3D Position { get; set; } = Point3D.Origin;
    public Vector3D Direction { get; set; } = -Vector3D.UnitZ;      // por defecto hacia abajo (gravedad)

    /// <summary>
    /// Magnitud nominal en unidades SI: N (puntual), N/m (lineal), N/m2 (area), Nm (momento).
    /// </summary>
    public double Magnitude { get; set; }

    /// <summary>Longitud o area de aplicacion para cargas distribuidas.</summary>
    public double? ExtentLength { get; set; }
    public double? ExtentWidth { get; set; }

    public double MomentMx { get; set; }    // Nm
    public double MomentMy { get; set; }    // Nm
    public double MomentMz { get; set; }    // Nm
}

public enum LoadApplicationKind
{
    Point,
    LineDistributed,
    AreaDistributed,
    Moment,
    PointWithMoments
}

/// <summary>
/// Combinacion de cargas segun norma. Factor de cada caso y tipo de ELU/ELS.
/// </summary>
public class LoadCombination : BimObject
{
    public override string ObjectType => "LoadCombination";
    public LimitStateKind LimitState { get; set; } = LimitStateKind.ULS;
    public DesignSituation Situation { get; set; } = DesignSituation.Persistent;
    public List<LoadCombinationFactor> Factors { get; set; } = new();
    public bool IsGenerated { get; set; }  // true si la genero el motor automaticamente
}

public class LoadCombinationFactor
{
    public Guid LoadCaseId { get; set; }
    public double Factor { get; set; }
    public CombinationRole Role { get; set; } = CombinationRole.Leading;
}

public enum LimitStateKind
{
    ULS,            // Estado Limite Ultimo
    SLS,            // Estado Limite de Servicio
    GEO,            // Estado Limite geotecnico (CTE DB-SE-C)
    EQU,            // Equilibrio (vuelco)
    STR,            // Resistencia estructural
    UPL,            // Flotacion / subpresion
    HYD             // Erosion interna, sifonamiento
}

public enum DesignSituation
{
    Persistent,
    Transient,
    Accidental,
    Seismic,
    QuasiPermanent,
    Frequent,
    Characteristic
}

public enum CombinationRole { Leading, Accompanying, Permanent }
