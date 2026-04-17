using CadZapatas.Core.Primitives;

namespace CadZapatas.Geometry.Solids;

/// <summary>
/// Caja rectangular alineada con ejes locales. Base de zapatas, vigas, muros rectos.
/// Orientacion: longitud en X local, ancho en Y local, canto en Z local.
/// La posicion corresponde al centro de la cara inferior.
/// </summary>
public class Box
{
    public Point3D Center { get; set; } = Point3D.Origin;
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double RotationZDegrees { get; set; }

    public double Volume => Length * Width * Height;
    public double BottomArea => Length * Width;
    public double LateralPerimeter => 2 * (Length + Width);

    public BoundingBox3D Bounds
    {
        get
        {
            // sin rotacion, caja alineada con centro en (cx,cy,cz+h/2)
            if (Math.Abs(RotationZDegrees) < 1e-9)
            {
                return new BoundingBox3D(
                    new Point3D(Center.X - Length / 2, Center.Y - Width / 2, Center.Z),
                    new Point3D(Center.X + Length / 2, Center.Y + Width / 2, Center.Z + Height));
            }
            // bounding box de la caja rotada
            var a = RotationZDegrees * Math.PI / 180.0;
            var cos = Math.Cos(a);
            var sin = Math.Sin(a);
            var halfLx = Math.Abs(cos) * Length / 2 + Math.Abs(sin) * Width / 2;
            var halfLy = Math.Abs(sin) * Length / 2 + Math.Abs(cos) * Width / 2;
            return new BoundingBox3D(
                new Point3D(Center.X - halfLx, Center.Y - halfLy, Center.Z),
                new Point3D(Center.X + halfLx, Center.Y + halfLy, Center.Z + Height));
        }
    }

    /// <summary>
    /// Devuelve los 8 vertices de la caja (en orden base inferior CCW, luego superior).
    /// </summary>
    public Point3D[] GetCorners()
    {
        var cos = Math.Cos(RotationZDegrees * Math.PI / 180.0);
        var sin = Math.Sin(RotationZDegrees * Math.PI / 180.0);
        var hl = Length / 2;
        var hw = Width / 2;

        Point3D RotatePlanar(double lx, double ly, double z)
        {
            var gx = Center.X + (lx * cos - ly * sin);
            var gy = Center.Y + (lx * sin + ly * cos);
            return new Point3D(gx, gy, z);
        }

        var z0 = Center.Z;
        var z1 = Center.Z + Height;
        return new[]
        {
            RotatePlanar(-hl, -hw, z0),
            RotatePlanar( hl, -hw, z0),
            RotatePlanar( hl,  hw, z0),
            RotatePlanar(-hl,  hw, z0),
            RotatePlanar(-hl, -hw, z1),
            RotatePlanar( hl, -hw, z1),
            RotatePlanar( hl,  hw, z1),
            RotatePlanar(-hl,  hw, z1),
        };
    }

    /// <summary>Centro geometrico del solido.</summary>
    public Point3D Centroid => new(Center.X, Center.Y, Center.Z + Height / 2);
}

/// <summary>
/// Cilindro vertical con base circular. Base de pilotes, micropilotes, columnas.
/// </summary>
public class Cylinder
{
    public Point3D BaseCenter { get; set; } = Point3D.Origin;
    public double Diameter { get; set; }
    public double Height { get; set; }     // negativa si va hacia abajo (pilotes)

    public double Radius => Diameter / 2;
    public double Volume => Math.PI * Radius * Radius * Math.Abs(Height);
    public double LateralArea => Math.PI * Diameter * Math.Abs(Height);
    public double BaseArea => Math.PI * Radius * Radius;

    public BoundingBox3D Bounds => new(
        new Point3D(BaseCenter.X - Radius, BaseCenter.Y - Radius, Math.Min(BaseCenter.Z, BaseCenter.Z + Height)),
        new Point3D(BaseCenter.X + Radius, BaseCenter.Y + Radius, Math.Max(BaseCenter.Z, BaseCenter.Z + Height)));
}

/// <summary>
/// Prisma extruido vertical a partir de una polilinea cerrada (planta).
/// Base de losas de cimentacion, muros de planta irregular.
/// </summary>
public class ExtrudedPolygon
{
    public List<Point2D> Outline { get; set; } = new();
    public List<List<Point2D>> Holes { get; set; } = new();
    public double BaseElevation { get; set; }
    public double Thickness { get; set; }

    public double Volume
    {
        get
        {
            var outer = PolygonMath.SignedArea(Outline);
            var holes = Holes.Sum(h => PolygonMath.SignedArea(h));
            return (Math.Abs(outer) - Math.Abs(holes)) * Thickness;
        }
    }

    public double PlanArea
    {
        get
        {
            var outer = Math.Abs(PolygonMath.SignedArea(Outline));
            var holes = Holes.Sum(h => Math.Abs(PolygonMath.SignedArea(h)));
            return outer - holes;
        }
    }
}
