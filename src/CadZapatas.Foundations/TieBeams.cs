using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Viga de atado entre dos cimentaciones (arriostramiento horizontal).
/// </summary>
public class TieBeam : StructuralElement
{
    public override string ObjectType => "TieBeam";
    public Guid StartFoundationId { get; set; }
    public Guid EndFoundationId { get; set; }
    public Point3D StartPoint { get; set; }
    public Point3D EndPoint { get; set; }
    public double Width { get; set; } = 0.40;
    public double Height { get; set; } = 0.40;

    public double Length => StartPoint.DistanceTo(EndPoint);
    public double VolumeConcrete => Width * Height * Length;

    public Box ToBox()
    {
        var v = EndPoint - StartPoint;
        var ang = Math.Atan2(v.Y, v.X) * 180 / Math.PI;
        return new Box
        {
            Center = new Point3D((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2,
                                  (StartPoint.Z + EndPoint.Z) / 2),
            Length = Length,
            Width = Width,
            Height = Height,
            RotationZDegrees = ang
        };
    }
}

/// <summary>
/// Viga centradora. Caso especifico de viga que restituye el momento de medianeria
/// al soporte interior adyacente. Siempre esta vinculada a una zapata de medianeria.
/// </summary>
public class CenteringBeam : TieBeam
{
    public override string ObjectType => "CenteringBeam";
    public Guid MedianeraFootingId { get; set; }
    public Guid InteriorFootingId { get; set; }

    public double DesignMoment_kNm { get; set; }
}
