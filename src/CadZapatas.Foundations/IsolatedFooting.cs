using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Zapata aislada rectangular. Apoya un soporte (pilar o muro corto).
/// La posicion InsertionPoint corresponde al centro de la cara inferior.
/// </summary>
public class IsolatedFooting : Foundation
{
    public override string ObjectType => "IsolatedFooting";

    public double Length { get; set; } = 1.50;    // a (direccion X local)
    public double Width { get; set; } = 1.50;     // b (direccion Y local)
    public double Thickness { get; set; } = 0.50; // h (canto)

    // Soporte sobre la zapata
    public double ColumnLengthX { get; set; } = 0.30;
    public double ColumnLengthY { get; set; } = 0.30;
    public ColumnShape ColumnShape { get; set; } = ColumnShape.Rectangular;
    public double ColumnEccentricityX { get; set; }
    public double ColumnEccentricityY { get; set; }

    /// <summary>True si la zapata es escalonada.</summary>
    public bool IsStepped { get; set; }
    public List<FootingStep> Steps { get; set; } = new();

    public Box ToBox() => new()
    {
        Center = InsertionPoint,
        Length = Length,
        Width = Width,
        Height = Thickness,
        RotationZDegrees = RotationDegrees
    };

    public double VolumeConcrete => Length * Width * Thickness;
    public double PlanArea => Length * Width;
    public double LeanConcreteVolume => (Length + 0.20) * (Width + 0.20) * LeanConcreteThickness; // vuelo 10 cm por lado
    public double FormworkArea => 2 * (Length + Width) * Thickness;
}

public enum ColumnShape { Rectangular, Circular, LShape, TShape }

public class FootingStep
{
    public double Height { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
}
