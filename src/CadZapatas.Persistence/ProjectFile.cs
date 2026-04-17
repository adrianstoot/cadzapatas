using CadZapatas.Core.Bim;

namespace CadZapatas.Persistence;

/// <summary>
/// Envoltorio serializable del proyecto con todos sus elementos BIM.
/// Se guarda como documento JSON dentro del archivo .czap (SQLite).
/// Los tipos polimorficos usan metadata de tipo en el discriminador.
/// </summary>
public class ProjectFile
{
    public string FormatVersion { get; set; } = "1.0";
    public string ApplicationVersion { get; set; } = "CadZapatas 0.1.0";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = new();

    // Los elementos se serializan como diccionario por tipo -> lista de objetos
    // Esto evita la complejidad de serializar jerarquias polimorficas sin TypeInfoResolver
    public Dictionary<string, List<object>> ElementsByType { get; set; } = new();

    // Configuracion del proyecto (normativa activa, unidades, etc.)
    public ProjectSettings Settings { get; set; } = new();
}

public class ProjectSettings
{
    public string UnitsSystem { get; set; } = "SI";
    public string AngleUnit { get; set; } = "deg";
    public string GeotechnicalNormCode { get; set; } = "CTE_DB_SE_C_2019";
    public string StructuralNormCode { get; set; } = "CE_RD_470_2021";
    public bool LegacyMode { get; set; }                   // true si se usa EHE-08
    public string DefaultConcreteClass { get; set; } = "HA-25";
    public string DefaultSteelGrade { get; set; } = "B500SD";
    public string DefaultExposureClass { get; set; } = "XC2";
    public double DefaultNominalCoverMm { get; set; } = 50.0;
}
