using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;

namespace CadZapatas.Retaining;

/// <summary>
/// Muro pantalla (diaphragm wall). Serie de paneles de hormigon armado ejecutados in situ
/// con lodos bentonticos, o pantalla de pilotes (secantes, tangentes, contiguos).
/// </summary>
public class DiaphragmWall : StructuralElement
{
    public override string ObjectType => "DiaphragmWall";
    public DiaphragmWallType Type { get; set; } = DiaphragmWallType.CastInPlace;
    public double Thickness { get; set; } = 0.60;           // espesor panel
    public double PanelLength { get; set; } = 2.50;         // longitud estandar de panel
    public double Depth { get; set; } = 15.0;               // profundidad total
    public double ToeEmbedment { get; set; } = 3.0;         // empotramiento bajo el fondo

    // Traza de la pantalla (polilinea en planta)
    public List<Point2D> Trace { get; set; } = new();

    // Niveles de excavacion por fases
    public List<ExcavationStage> Stages { get; set; } = new();

    public double TotalLength
    {
        get
        {
            double l = 0;
            for (int i = 0; i < Trace.Count - 1; i++) l += Trace[i].DistanceTo(Trace[i + 1]);
            return l;
        }
    }

    public double VolumeConcrete => Thickness * Depth * TotalLength;
    public double FaceArea => Depth * TotalLength;
}

public enum DiaphragmWallType
{
    CastInPlace,            // panel hormigonado in situ
    SecantPile,             // pilotes secantes
    TangentPile,            // pilotes tangentes
    ContiguousPile          // pilotes contiguos (con separacion)
}

/// <summary>
/// Fase de excavacion. Cada fase define profundidad excavada y elementos de soporte actuales.
/// </summary>
public class ExcavationStage
{
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public double ExcavationElevation { get; set; }
    public List<Guid> ActiveAnchorIds { get; set; } = new();
    public List<Guid> ActiveStrutIds { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Anclaje al terreno. Tendon activo (pretensado) o pasivo.
/// </summary>
public class GroundAnchor : StructuralElement
{
    public override string ObjectType => "GroundAnchor";
    public GroundAnchorKind Kind { get; set; } = GroundAnchorKind.TemporaryActive;

    public Point3D HeadPoint { get; set; } = Point3D.Origin;
    public double InclinationDegrees { get; set; } = 20;
    public double AzimuthDegrees { get; set; }

    public double FreeLength { get; set; } = 6.0;
    public double BondLength { get; set; } = 8.0;
    public double TotalLength => FreeLength + BondLength;

    public AnchorTendonType TendonType { get; set; } = AnchorTendonType.Strand;
    public int NumberOfStrands { get; set; } = 4;
    public double StrandDiameterMm { get; set; } = 15.2;
    public string SteelGrade { get; set; } = "Y1860";

    public double DesignLoad_kN { get; set; } = 400;
    public double ProofLoad_kN { get; set; } = 500;
    public double LockOffLoad_kN { get; set; } = 360;

    public AnchorCorrosionProtection Corrosion { get; set; } = AnchorCorrosionProtection.Single;

    public Guid? WallId { get; set; }
    public int AnchorLevel { get; set; }

    public double HoleDiameter { get; set; } = 0.150;
}

public enum GroundAnchorKind
{
    TemporaryActive,
    PermanentActive,
    Passive
}

public enum AnchorTendonType { Strand, Bar }
public enum AnchorCorrosionProtection { Single, Double }

/// <summary>
/// Puntal o codal (strut). Apoyo horizontal durante excavacion.
/// </summary>
public class Strut : StructuralElement
{
    public override string ObjectType => "Strut";
    public Point3D StartPoint { get; set; }
    public Point3D EndPoint { get; set; }
    public string SectionDesignation { get; set; } = "HEB-300";
    public double PreloadKn { get; set; }
    public double Length => StartPoint.DistanceTo(EndPoint);
}
