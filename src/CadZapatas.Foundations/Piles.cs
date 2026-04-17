using CadZapatas.Core.Primitives;
using CadZapatas.Geometry.Solids;

namespace CadZapatas.Foundations;

/// <summary>
/// Pilote (perforado o prefabricado). Representado como cilindro vertical con cabeza
/// en la elevacion del encepado o terreno.
/// </summary>
public class Pile : Foundation
{
    public override string ObjectType => "Pile";
    public PileKind Kind { get; set; } = PileKind.BoredInSitu;
    public double Diameter { get; set; } = 0.60;
    public double Length { get; set; } = 12.0;

    /// <summary>Elevacion de la cabeza (parte superior).</summary>
    public double HeadElevation { get; set; }

    /// <summary>Elevacion de la punta.</summary>
    public double TipElevation => HeadElevation - Length;

    public double InclinationDegrees { get; set; }
    public double InclinationAzimuthDegrees { get; set; }
    public Guid? PileCapId { get; set; }
    public string ExecutionMethod { get; set; } = "CPI-4";   // CPI-2 a CPI-8, hincado, barrenado...

    public double VolumeConcrete => Math.PI * Diameter * Diameter / 4.0 * Length;
    public double LateralSurfaceArea => Math.PI * Diameter * Length;
    public double TipArea => Math.PI * Diameter * Diameter / 4.0;

    public Cylinder ToCylinder() => new()
    {
        BaseCenter = new Point3D(InsertionPoint.X, InsertionPoint.Y, TipElevation),
        Diameter = Diameter,
        Height = Length
    };
}

public enum PileKind
{
    BoredInSitu,            // perforado
    DrivenPrecast,          // prefabricado hincado
    Micropile,              // micropilote
    CFA,                    // continuous flight auger (barrena continua)
    HelicalScrew,
    SteelH,
    SteelPipe
}

/// <summary>
/// Micropilote: diametros pequenos, normalmente inyectados.
/// </summary>
public class Micropile : Pile
{
    public override string ObjectType => "Micropile";
    public double CasingDiameter { get; set; } = 0.150;      // armadura tubular
    public double CasingWallThickness { get; set; } = 0.010;
    public double FreeLength { get; set; }                   // tramo libre
    public double BondLength { get; set; }                   // tramo activo (inyeccion)
    public MicropileInjection Injection { get; set; } = MicropileInjection.IGU;
    public string GroutGrade { get; set; } = "Lechada 1:2 c:a";
}

public enum MicropileInjection
{
    IGU,        // Inyeccion global unica
    IRS,        // Inyeccion repetitiva y selectiva
    IR          // Inyeccion repetitiva
}

/// <summary>
/// Encepado sobre pilotes. Reparte las cargas del soporte entre los pilotes.
/// </summary>
public class PileCap : Foundation
{
    public override string ObjectType => "PileCap";
    public double Length { get; set; } = 2.00;
    public double Width { get; set; } = 2.00;
    public double Thickness { get; set; } = 0.90;
    public List<Guid> PileIds { get; set; } = new();
    public double PileHeadEmbedment { get; set; } = 0.10;  // m que penetra el pilote en el encepado

    public double VolumeConcrete => Length * Width * Thickness;
    public double PlanArea => Length * Width;
    public double LeanConcreteVolume => (Length + 0.20) * (Width + 0.20) * LeanConcreteThickness;

    public Box ToBox() => new()
    {
        Center = InsertionPoint,
        Length = Length,
        Width = Width,
        Height = Thickness,
        RotationZDegrees = RotationDegrees
    };
}
