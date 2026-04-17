using CadZapatas.Core.Primitives;

namespace CadZapatas.Geometry;

/// <summary>
/// Algoritmos geometricos 2D sin dependencia externa:
/// - area con signo (gauss / shoelace);
/// - centroide;
/// - perimetro;
/// - punto en poligono (ray casting);
/// - orientacion (CCW / CW).
/// </summary>
public static class PolygonMath
{
    public static double SignedArea(IReadOnlyList<Point2D> pts)
    {
        if (pts.Count < 3) return 0;
        double a = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            var p1 = pts[i];
            var p2 = pts[(i + 1) % pts.Count];
            a += p1.X * p2.Y - p2.X * p1.Y;
        }
        return a / 2.0;
    }

    public static Point2D Centroid(IReadOnlyList<Point2D> pts)
    {
        if (pts.Count == 0) return Point2D.Origin;
        if (pts.Count < 3)
        {
            var mx = pts.Average(p => p.X);
            var my = pts.Average(p => p.Y);
            return new Point2D(mx, my);
        }
        double cx = 0, cy = 0, area = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            var p1 = pts[i];
            var p2 = pts[(i + 1) % pts.Count];
            var cross = p1.X * p2.Y - p2.X * p1.Y;
            area += cross;
            cx += (p1.X + p2.X) * cross;
            cy += (p1.Y + p2.Y) * cross;
        }
        area /= 2;
        if (Math.Abs(area) < 1e-12) return new Point2D(pts.Average(p => p.X), pts.Average(p => p.Y));
        return new Point2D(cx / (6 * area), cy / (6 * area));
    }

    public static double Perimeter(IReadOnlyList<Point2D> pts)
    {
        if (pts.Count < 2) return 0;
        double p = 0;
        for (int i = 0; i < pts.Count; i++)
            p += pts[i].DistanceTo(pts[(i + 1) % pts.Count]);
        return p;
    }

    public static bool IsCounterClockwise(IReadOnlyList<Point2D> pts) => SignedArea(pts) > 0;

    /// <summary>
    /// Ray casting horizontal. Incluye puntos sobre borde como dentro.
    /// </summary>
    public static bool Contains(IReadOnlyList<Point2D> poly, Point2D p)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            var xi = poly[i].X; var yi = poly[i].Y;
            var xj = poly[j].X; var yj = poly[j].Y;
            bool intersect = ((yi > p.Y) != (yj > p.Y)) &&
                             (p.X < (xj - xi) * (p.Y - yi) / (yj - yi + 1e-30) + xi);
            if (intersect) inside = !inside;
        }
        return inside;
    }
}
