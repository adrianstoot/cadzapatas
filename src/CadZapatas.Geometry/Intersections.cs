using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Geometry;

/// <summary>
/// Comprobaciones de interferencia entre solidos. Resuelto a bounding-box y,
/// cuando aplica, a una comprobacion 2D mas fina.
/// </summary>
public static class Intersections
{
    public static bool BoundingBoxesOverlap(BoundingBox3D a, BoundingBox3D b, double tolerance = 0)
    {
        if (a.Max.X + tolerance < b.Min.X || a.Min.X - tolerance > b.Max.X) return false;
        if (a.Max.Y + tolerance < b.Min.Y || a.Min.Y - tolerance > b.Max.Y) return false;
        if (a.Max.Z + tolerance < b.Min.Z || a.Min.Z - tolerance > b.Max.Z) return false;
        return true;
    }

    public static bool BoxesIntersect(Box a, Box b, double tolerance = 0.001)
        => BoundingBoxesOverlap(a.Bounds, b.Bounds, tolerance);

    public static bool CylinderInsidePolygon(Cylinder c, IReadOnlyList<Point2D> poly)
    {
        var p = new Point2D(c.BaseCenter.X, c.BaseCenter.Y);
        return PolygonMath.Contains(poly, p);
    }

    /// <summary>
    /// Distancia minima entre dos cilindros verticales en su proyeccion en planta.
    /// No contempla traslape vertical.
    /// </summary>
    public static double PlanarDistance(Cylinder a, Cylinder b)
    {
        var pa = new Point2D(a.BaseCenter.X, a.BaseCenter.Y);
        var pb = new Point2D(b.BaseCenter.X, b.BaseCenter.Y);
        return Math.Max(0, pa.DistanceTo(pb) - a.Radius - b.Radius);
    }
}
