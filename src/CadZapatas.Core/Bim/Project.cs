namespace CadZapatas.Core.Bim;

/// <summary>
/// Proyecto BIM. Raiz de la jerarquia. Contiene referencias a normativa, unidades,
/// niveles, ejes, fases y elementos estructurales y geotecnicos.
/// </summary>
public class Project : BimObject
{
    public override string ObjectType => "Project";

    public string? ClientName { get; set; }
    public string? EngineerName { get; set; }
    public string? Address { get; set; }
    public string? CityProvince { get; set; }
    public string? ReferenceNumber { get; set; }

    /// <summary>Norma principal de cimentaciones (CTE DB-SE-C).</summary>
    public string GeotechnicalNormCode { get; set; } = "CTE_DB_SE_C_2019";

    /// <summary>Norma principal estructural (Codigo Estructural RD 470/2021).</summary>
    public string StructuralNormCode { get; set; } = "CE_RD_470_2021";

    /// <summary>Si true, permite usar modulo de compatibilidad EHE-08 para importar proyectos antiguos.</summary>
    public bool EheCompatibilityMode { get; set; } = false;

    public int DesignLifeYears { get; set; } = 50;

    /// <summary>Grupo de terreno segun CTE DB-SE-C 3.1.</summary>
    public string TerrainGroup { get; set; } = "T2"; // T1, T2 o T3

    /// <summary>Tipo de construccion segun CTE DB-SE-C 3.1.</summary>
    public string ConstructionType { get; set; } = "C-2"; // C-0 ... C-4

    public List<Level> Levels { get; set; } = new();
    public List<Grid> Grids { get; set; } = new();
    public List<Phase> Phases { get; set; } = new();
    public List<ProjectVariant> Variants { get; set; } = new();

    public Project()
    {
        // Fase por defecto
        Phases.Add(new Phase { Code = "F-01", Name = "Construccion", SequenceOrder = 1 });
    }
}

/// <summary>Nivel de referencia (cota).</summary>
public class Level : BimObject
{
    public override string ObjectType => "Level";
    public double Elevation { get; set; }  // metros sobre referencia del proyecto
    public bool IsReference { get; set; }
    public LevelType Type { get; set; } = LevelType.Foundation;
}

public enum LevelType { Terrain, Foundation, Floor, Roof, Excavation }

/// <summary>Eje o malla de referencia.</summary>
public class Grid : BimObject
{
    public override string ObjectType => "Grid";
    public GridDirection Direction { get; set; }
    public string Label { get; set; } = string.Empty;      // A, B, C...  o  1, 2, 3...
    public double Offset { get; set; }                     // distancia desde origen
    public double Length { get; set; } = 20.0;
    public double? AngleDegrees { get; set; }              // para ejes oblicuos
}

public enum GridDirection { Vertical, Horizontal, Oblique }

/// <summary>Fase de obra o fase constructiva.</summary>
public class Phase : BimObject
{
    public override string ObjectType => "Phase";
    public int SequenceOrder { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}

/// <summary>Variante del proyecto (para opciones de diseno).</summary>
public class ProjectVariant : BimObject
{
    public override string ObjectType => "ProjectVariant";
    public bool IsActive { get; set; }
}
