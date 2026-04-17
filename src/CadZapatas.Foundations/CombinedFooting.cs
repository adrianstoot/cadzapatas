using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Zapata combinada bajo dos o mas pilares. Dimensionada para redistribuir la presion.
/// </summary>
public class CombinedFooting : Foundation
{
    public override string ObjectType => "CombinedFooting";
    public double Length { get; set; } = 4.00;
    public double Width { get; set; } = 1.80;
    public double Thickness { get; set; } = 0.60;
    public CombinedFootingShape Shape { get; set; } = CombinedFootingShape.Rectangular;
    public List<ColumnOnFooting> Columns { get; set; } = new();

    public double VolumeConcrete => Length * Width * Thickness;
    public double PlanArea => Length * Width;
    public double LeanConcreteVolume => (Length + 0.20) * (Width + 0.20) * LeanConcreteThickness;
    public double FormworkArea => 2 * (Length + Width) * Thickness;

    public Box ToBox() => new() { Center = InsertionPoint, Length = Length, Width = Width, Height = Thickness, RotationZDegrees = RotationDegrees };
}

public enum CombinedFootingShape { Rectangular, Trapezoidal, TShape }

public class ColumnOnFooting
{
    public Point3D Position { get; set; } = Point3D.Origin;
    public double SizeX { get; set; } = 0.30;
    public double SizeY { get; set; } = 0.30;
    public double N_kN { get; set; }
    public double Mx_kNm { get; set; }
    public double My_kNm { get; set; }
}

/// <summary>
/// Zapata de medianeria. Requiere siempre una viga centradora vinculada.
/// </summary>
public class MedianeraFooting : Foundation
{
    public override string ObjectType => "MedianeraFooting";
    public double Length { get; set; } = 1.80;
    public double Width { get; set; } = 1.20;
    public double Thickness { get; set; } = 0.50;
    /// <summary>Desplazamiento del soporte al borde de medianera (siempre en +X local).</summary>
    public double EdgeOffset { get; set; } = 0.15;
    public Guid? CenteringBeamId { get; set; }

    public double VolumeConcrete => Length * Width * Thickness;
    public double PlanArea => Length * Width;
    public double LeanConcreteVolume => (Length + 0.20) * (Width + 0.20) * LeanConcreteThickness;

    public Box ToBox() => new() { Center = InsertionPoint, Length = Length, Width = Width, Height = Thickness, RotationZDegrees = RotationDegrees };
}
