namespace CadZapatas.Core.Primitives;

/// <summary>
/// Punto 3D con coordenadas en metros.
/// </summary>
public readonly record struct Point3D(double X, double Y, double Z)
{
    public static Point3D Origin => new(0, 0, 0);

    public static Point3D operator +(Point3D a, Vector3D v) => new(a.X + v.X, a.Y + v.Y, a.Z + v.Z);
    public static Point3D operator -(Point3D a, Vector3D v) => new(a.X - v.X, a.Y - v.Y, a.Z - v.Z);
    public static Vector3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public double DistanceTo(Point3D other) => (this - other).Length;

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}

/// <summary>
/// Vector 3D.
/// </summary>
public readonly record struct Vector3D(double X, double Y, double Z)
{
    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double LengthSquared => X * X + Y * Y + Z * Z;

    public Vector3D Normalized
    {
        get
        {
            var l = Length;
            if (l < 1e-12) return Zero;
            return new Vector3D(X / l, Y / l, Z / l);
        }
    }

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3D operator *(Vector3D a, double s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3D operator *(double s, Vector3D a) => a * s;
    public static Vector3D operator /(Vector3D a, double s) => new(a.X / s, a.Y / s, a.Z / s);
    public static Vector3D operator -(Vector3D a) => new(-a.X, -a.Y, -a.Z);

    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    public Vector3D Cross(Vector3D other) => new(
        Y * other.Z - Z * other.Y,
        Z * other.X - X * other.Z,
        X * other.Y - Y * other.X);
}

public readonly record struct Point2D(double X, double Y)
{
    public static Point2D Origin => new(0, 0);
    public double DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    public override string ToString() => $"({X:F3}, {Y:F3})";
}

public readonly record struct BoundingBox3D(Point3D Min, Point3D Max)
{
    public double SizeX => Max.X - Min.X;
    public double SizeY => Max.Y - Min.Y;
    public double SizeZ => Max.Z - Min.Z;
    public Point3D Center => new((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2, (Min.Z + Max.Z) / 2);

    public BoundingBox3D Expand(BoundingBox3D other) => new(
        new Point3D(Math.Min(Min.X, other.Min.X), Math.Min(Min.Y, other.Min.Y), Math.Min(Min.Z, other.Min.Z)),
        new Point3D(Math.Max(Max.X, other.Max.X), Math.Max(Max.Y, other.Max.Y), Math.Max(Max.Z, other.Max.Z)));

    public bool Contains(Point3D p) =>
        p.X >= Min.X && p.X <= Max.X &&
        p.Y >= Min.Y && p.Y <= Max.Y &&
        p.Z >= Min.Z && p.Z <= Max.Z;
}
