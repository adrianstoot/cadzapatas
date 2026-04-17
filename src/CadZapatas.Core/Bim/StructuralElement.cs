using CadZapatas.Core.Primitives;

namespace CadZapatas.Core.Bim;

/// <summary>
/// Base comun de todo elemento estructural (cimentacion, contencion, armadura, etc.).
/// Aporta geometria, posicion y referencia de materiales.
/// </summary>
public abstract class StructuralElement : BimObject
{
    /// <summary>Posicion de insercion (centro o punto base segun el tipo concreto).</summary>
    public Point3D InsertionPoint { get; set; } = Point3D.Origin;

    /// <summary>Angulo de rotacion en planta (grados), positivo antihorario.</summary>
    public double RotationDegrees { get; set; }

    /// <summary>Codigo del hormigon asignado (HA-25, HA-30, HM-20...).</summary>
    public string ConcreteClass { get; set; } = "HA-25";

    /// <summary>Codigo del acero de armaduras pasivas (B500SD, B500S...).</summary>
    public string RebarSteelGrade { get; set; } = "B500SD";

    /// <summary>Clase de exposicion segun Codigo Estructural (XC2 tipico en cimentaciones).</summary>
    public string ExposureClass { get; set; } = "XC2";

    /// <summary>Clase estructural segun Codigo Estructural (tipica S4 = 50 anos).</summary>
    public string StructuralClass { get; set; } = "S4";

    /// <summary>Recubrimiento nominal en metros.</summary>
    public double NominalCover { get; set; } = 0.05;

    /// <summary>Identificador del estrato geotecnico de apoyo, si aplica.</summary>
    public Guid? BearingSoilLayerId { get; set; }

    /// <summary>Identificadores de cargas aplicadas.</summary>
    public List<Guid> LoadIds { get; set; } = new();

    /// <summary>Resultados de comprobaciones normativas ultimas.</summary>
    public List<NormativeCheckResult> LastCheckResults { get; set; } = new();
}

/// <summary>
/// Resultado de una comprobacion normativa concreta (ligero, para listados).
/// Los detalles completos van en CalcTrace del motor normativo.
/// </summary>
public class NormativeCheckResult
{
    public string CheckId { get; set; } = string.Empty;        // identificador de la regla
    public string NormCode { get; set; } = string.Empty;       // CTE_DB_SE_C, CE_RD_470_2021...
    public string Article { get; set; } = string.Empty;        // 4.3.1, Art. 44...
    public string CheckName { get; set; } = string.Empty;
    public double Utilization { get; set; }                    // demanda / capacidad  [0-1 ok, >1 falla]
    public CheckVerdict Verdict { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum CheckVerdict { Pass, Warning, Fail, NotApplicable }
