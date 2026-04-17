using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Retaining;

/// <summary>
/// Muro de contencion en ménsula (cantilever) o con contrafuertes.
/// Seccion en L o T-invertida segun configuracion de puntera, talon y tacon.
/// </summary>
public class RetainingWall : StructuralElement
{
    public override string ObjectType => "RetainingWall";

    public RetainingWallKind Kind { get; set; } = RetainingWallKind.Cantilever;

    // Geometria del alzado
    public double Height { get; set; } = 4.00;                       // altura total del muro sobre el cimiento
    public double StemThicknessTop { get; set; } = 0.30;
    public double StemThicknessBottom { get; set; } = 0.30;

    // Geometria del cimiento
    public double ToeLength { get; set; } = 0.80;       // puntera (en el lado del relleno)
    public double HeelLength { get; set; } = 1.20;      // talon (en el lado retenido)
    public double FoundationThickness { get; set; } = 0.50;
    public double KeyDepth { get; set; }                // tacón (0 = sin tacón)
    public double KeyWidth { get; set; } = 0.30;

    // Longitud del muro (direccion perpendicular a la seccion)
    public Point3D StartPoint { get; set; }
    public Point3D EndPoint { get; set; }

    public double WallLength => StartPoint.DistanceTo(EndPoint);

    // Relleno trasdos
    public double BackfillSlopeDegrees { get; set; }
    public Guid? BackfillSoilId { get; set; }           // suelo del trasdos (puede diferir del natural)
    public Guid? FoundationSoilId { get; set; }         // suelo de apoyo

    // Sobrecarga en trasdos
    public double SurchargeKPa { get; set; }

    public bool HasDrainage { get; set; } = true;
    public bool HasWaterproofing { get; set; } = true;

    public double StemVolume
    {
        get
        {
            var avg = (StemThicknessTop + StemThicknessBottom) / 2.0;
            return avg * Height * WallLength;
        }
    }

    public double FoundationVolume
        => (ToeLength + StemThicknessBottom + HeelLength) * FoundationThickness * WallLength
           + KeyDepth * KeyWidth * WallLength;

    public double TotalConcreteVolume => StemVolume + FoundationVolume;

    public double BaseWidth => ToeLength + StemThicknessBottom + HeelLength;

    public double BackfaceExposedArea => Height * WallLength;
}

public enum RetainingWallKind
{
    Gravity,                // por peso propio, sin armadura
    Cantilever,             // en ménsula, armadura en alzado y cimiento
    Counterfort,            // con contrafuertes
    Basement                // muro de sótano (con apoyo superior del forjado)
}

/// <summary>
/// Muro de sotano. Restringido en cabeza (apoyo del forjado). Genera empujes horizontales
/// que se transmiten al forjado y a la cimentacion.
/// </summary>
public class BasementWall : RetainingWall
{
    public override string ObjectType => "BasementWall";
    public int SupportedFloorCount { get; set; } = 1;
    public double FirstFloorSlabElevation { get; set; }
}
