using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Zapata corrida bajo muro o serie de pilares.
/// El eje longitudinal de la zapata va de StartPoint a EndPoint.
/// </summary>
public class StripFooting : Foundation
{
    public override string ObjectType => "StripFooting";

    public Point3D StartPoint { get; set; }
    public Point3D EndPoint { get; set; }
    public double Width { get; set; } = 0.80;
    public double Thickness { get; set; } = 0.40;
    public double WallThickness { get; set; } = 0.25;

    /// <summary>Desfase transversal del muro respecto al eje de la zapata (m). 0 = centrado.</summary>
    public double WallOffset { get; set; }

    public double Length => StartPoint.DistanceTo(EndPoint);
    public double VolumeConcrete => Width * Thickness * Length;
    public double PlanArea => Width * Length;
    public double LeanConcreteVolume => (Width + 0.20) * LeanConcreteThickness * Length;
    public double FormworkArea => 2 * Thickness * Length;

    public Box ToBox()
    {
        // caja cuya direccion longitudinal alinea Start -> End
        var v = EndPoint - StartPoint;
        var angle = Math.Atan2(v.Y, v.X) * 180.0 / Math.PI;
        return new Box
        {
            Center = new Point3D((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2, FoundingElevation),
            Length = Length,
            Width = Width,
            Height = Thickness,
            RotationZDegrees = angle
        };
    }
}
