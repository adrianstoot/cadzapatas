using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;

namespace CadZapatas.Geotechnics;

/// <summary>
/// Sondeo geotecnico. Punto de reconocimiento con ubicacion, profundidad, metodo y datos asociados.
/// </summary>
public class Borehole : BimObject
{
    public override string ObjectType => "Borehole";

    public Point3D Location { get; set; } = Point3D.Origin;  // cabeza del sondeo
    public double Depth { get; set; }                         // profundidad alcanzada
    public BoreholeMethod Method { get; set; } = BoreholeMethod.RotaryCore;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Equipment { get; set; }
    public string? Operator { get; set; }

    /// <summary>Nivel freatico al perforar (elevacion).</summary>
    public double? WaterTableAtDrilling { get; set; }

    /// <summary>Capas detectadas en el sondeo, ordenadas por profundidad.</summary>
    public List<BoreholeLayerOccurrence> Layers { get; set; } = new();

    public List<SptTest> SptTests { get; set; } = new();
    public List<LabTest> LabTests { get; set; } = new();

    public string? Observations { get; set; }
}

public enum BoreholeMethod
{
    RotaryCore,
    Percussion,
    Mixed,
    DirectPush,
    HandAuger,
    TestPit
}

/// <summary>
/// Capa detectada en un sondeo concreto (instancia de un estrato).
/// </summary>
public class BoreholeLayerOccurrence
{
    public Guid SoilLayerId { get; set; }
    public double TopDepth { get; set; }    // profundidad desde cabeza (positiva hacia abajo)
    public double BottomDepth { get; set; }
    public string? Description { get; set; }
    public string? Recovery { get; set; }
    public string? RQD { get; set; }         // rock quality designation (solo rocas)
}

/// <summary>
/// Ensayo SPT individual.
/// </summary>
public class SptTest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BoreholeId { get; set; }
    public double Depth { get; set; }
    public int N1 { get; set; }     // golpes tramos 15 cm
    public int N2 { get; set; }
    public int N3 { get; set; }

    /// <summary>N = N2 + N3 (despreciando primeros 15 cm).</summary>
    public int NRaw => N2 + N3;

    public double EnergyRatioPercent { get; set; } = 60; // Er
    public double CorrectionCn { get; set; } = 1.0;       // correccion por presion efectiva
    public int N60 => (int)Math.Round(NRaw * EnergyRatioPercent / 60.0);
    public int N1_60 => (int)Math.Round(N60 * CorrectionCn);

    public bool RefusalReached { get; set; }              // rechazo
    public string? SamplerType { get; set; }              // standard, modified, split spoon
    public string? Observations { get; set; }
}

/// <summary>
/// Ensayo de laboratorio. Tipo abierto mediante diccionario de resultados.
/// </summary>
public class LabTest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BoreholeId { get; set; }
    public string SampleCode { get; set; } = string.Empty;
    public double Depth { get; set; }
    public LabTestType TestType { get; set; } = LabTestType.Granulometry;
    public Dictionary<string, double> Results { get; set; } = new();   // "LL", "LP", "IP", "w", "gamma_d", "qu"...
    public string? LabName { get; set; }
    public DateTime? TestDate { get; set; }
    public string? Standard { get; set; }      // UNE, ASTM...
    public string? Observations { get; set; }
}

public enum LabTestType
{
    Granulometry,
    AtterbergLimits,
    NaturalMoisture,
    Density,
    TriaxialCU,
    TriaxialCD,
    TriaxialUU,
    Oedometer,
    DirectShear,
    UnconfinedCompression,
    Proctor,
    CBR,
    OrganicContent,
    SulfateContent,
    pH,
    Resistivity,
    FreeSwelling,
    Collapse
}
