using CadZapatas.Core.Primitives;
using CadZapatas.Geometry;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Losa de cimentacion. Definida por un contorno poligonal cerrado, con huecos opcionales.
/// Admite refuerzos locales (capiteles bajo pilares, zunchos perimetrales).
/// </summary>
public class MatFoundation : Foundation
{
    public override string ObjectType => "MatFoundation";
    public List<Point2D> Outline { get; set; } = new();
    public List<List<Point2D>> Holes { get; set; } = new();
    public double Thickness { get; set; } = 0.60;

    public double BaseElevation => FoundingElevation;

    public List<LocalThickening> LocalThickenings { get; set; } = new();
    public List<PerimetralBeam> PerimeterBeams { get; set; } = new();

    public double PlanArea
    {
        get
        {
            var outer = Math.Abs(PolygonMath.SignedArea(Outline));
            var holes = Holes.Sum(h => Math.Abs(PolygonMath.SignedArea(h)));
            return outer - holes;
        }
    }

    public double VolumeConcrete => PlanArea * Thickness + LocalThickenings.Sum(l => l.ExtraVolume);

    public ExtrudedPolygon ToExtrudedPolygon() => new()
    {
        Outline = Outline,
        Holes = Holes,
        BaseElevation = FoundingElevation,
        Thickness = Thickness
    };
}

/// <summary>Refuerzo local bajo pilar (capitel).</summary>
public class LocalThickening
{
    public Point2D Center { get; set; } = Point2D.Origin;
    public double Length { get; set; } = 1.50;
    public double Width { get; set; } = 1.50;
    public double ExtraThickness { get; set; } = 0.20;

    public double ExtraVolume => Length * Width * ExtraThickness;
}

/// <summary>Zuncho perimetral de refuerzo.</summary>
public class PerimetralBeam
{
    public double Width { get; set; } = 0.40;
    public double ExtraDepth { get; set; } = 0.30;
}
