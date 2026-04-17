using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Geometry.Meshing;

/// <summary>
/// Malla triangular para volcado al viewport 3D.
/// </summary>
public class TriangleMesh
{
    public List<Point3D> Positions { get; } = new();
    public List<int> TriangleIndices { get; } = new();
    public List<Vector3D> Normals { get; } = new();

    public int AddVertex(Point3D p, Vector3D? normal = null)
    {
        Positions.Add(p);
        Normals.Add(normal ?? Vector3D.UnitZ);
        return Positions.Count - 1;
    }

    public void AddTriangle(int i0, int i1, int i2)
    {
        TriangleIndices.Add(i0);
        TriangleIndices.Add(i1);
        TriangleIndices.Add(i2);
    }

    public void AddQuad(int i0, int i1, int i2, int i3)
    {
        AddTriangle(i0, i1, i2);
        AddTriangle(i0, i2, i3);
    }
}

/// <summary>
/// Constructor de mallas para solidos sencillos (Box, Cylinder, ExtrudedPolygon).
/// </summary>
public static class MeshBuilder
{
    public static TriangleMesh BuildBoxMesh(Box box)
    {
        var mesh = new TriangleMesh();
        var c = box.GetCorners(); // 0..3 inferior, 4..7 superior

        foreach (var p in c) mesh.AddVertex(p);

        // Inferior (mirando -Z)
        mesh.AddQuad(3, 2, 1, 0);
        // Superior (mirando +Z)
        mesh.AddQuad(4, 5, 6, 7);
        // Lados
        mesh.AddQuad(0, 1, 5, 4);
        mesh.AddQuad(1, 2, 6, 5);
        mesh.AddQuad(2, 3, 7, 6);
        mesh.AddQuad(3, 0, 4, 7);

        return mesh;
    }

    public static TriangleMesh BuildCylinderMesh(Cylinder cyl, int segments = 24)
    {
        var mesh = new TriangleMesh();
        double r = cyl.Radius;
        var baseCenter = mesh.AddVertex(cyl.BaseCenter);
        var topCenter = mesh.AddVertex(new Point3D(cyl.BaseCenter.X, cyl.BaseCenter.Y, cyl.BaseCenter.Z + cyl.Height));

        int[] baseIdx = new int[segments];
        int[] topIdx = new int[segments];
        for (int i = 0; i < segments; i++)
        {
            double a = i * 2 * Math.PI / segments;
            double x = cyl.BaseCenter.X + r * Math.Cos(a);
            double y = cyl.BaseCenter.Y + r * Math.Sin(a);
            baseIdx[i] = mesh.AddVertex(new Point3D(x, y, cyl.BaseCenter.Z));
            topIdx[i] = mesh.AddVertex(new Point3D(x, y, cyl.BaseCenter.Z + cyl.Height));
        }

        for (int i = 0; i < segments; i++)
        {
            int i1 = (i + 1) % segments;
            // base triangle fan
            mesh.AddTriangle(baseCenter, baseIdx[i1], baseIdx[i]);
            // top triangle fan
            mesh.AddTriangle(topCenter, topIdx[i], topIdx[i1]);
            // side quad
            mesh.AddQuad(baseIdx[i], baseIdx[i1], topIdx[i1], topIdx[i]);
        }
        return mesh;
    }

    public static TriangleMesh BuildExtrudedMesh(ExtrudedPolygon poly)
    {
        var mesh = new TriangleMesh();
        if (poly.Outline.Count < 3) return mesh;

        var bottomIdx = new int[poly.Outline.Count];
        var topIdx = new int[poly.Outline.Count];
        for (int i = 0; i < poly.Outline.Count; i++)
        {
            bottomIdx[i] = mesh.AddVertex(new Point3D(poly.Outline[i].X, poly.Outline[i].Y, poly.BaseElevation));
            topIdx[i] = mesh.AddVertex(new Point3D(poly.Outline[i].X, poly.Outline[i].Y, poly.BaseElevation + poly.Thickness));
        }

        // Caras laterales
        for (int i = 0; i < poly.Outline.Count; i++)
        {
            int i1 = (i + 1) % poly.Outline.Count;
            mesh.AddQuad(bottomIdx[i], bottomIdx[i1], topIdx[i1], topIdx[i]);
        }

        // Cara inferior y superior: triangulacion simple en abanico desde el centroide.
        // Suficiente para visualizar convexas; para concavas se podria usar ear-clipping (no critico para MVP).
        var centroid = PolygonMath.Centroid(poly.Outline);
        int cBot = mesh.AddVertex(new Point3D(centroid.X, centroid.Y, poly.BaseElevation));
        int cTop = mesh.AddVertex(new Point3D(centroid.X, centroid.Y, poly.BaseElevation + poly.Thickness));
        for (int i = 0; i < poly.Outline.Count; i++)
        {
            int i1 = (i + 1) % poly.Outline.Count;
            mesh.AddTriangle(cBot, bottomIdx[i1], bottomIdx[i]);
            mesh.AddTriangle(cTop, topIdx[i], topIdx[i1]);
        }

        return mesh;
    }
}
