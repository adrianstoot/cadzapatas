using CadZapatas.Core.Primitives;

namespace CadZapatas.Reinforcement;

/// <summary>
/// Malla electrosoldada (mallazo). Armadura prefabricada de barras soldadas en dos direcciones.
/// Designacion comercial: ME Ø/Ø - sL x sT - A x B - Grade (ej. ME 8/8 - 150x150 - 6x2.20 - B500T).
/// Referencia: Codigo Estructural art. 33 y Anejo 11; UNE-EN ISO 3766.
/// </summary>
public class RebarMesh
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Mark { get; set; } = string.Empty;
    public string Designation { get; set; } = "ME 8-8-150-150-B500T";

    /// <summary>Diametro de las barras en la direccion longitudinal (mm).</summary>
    public int LongitudinalDiameterMm { get; set; } = 8;

    /// <summary>Diametro en la direccion transversal (mm).</summary>
    public int TransverseDiameterMm { get; set; } = 8;

    /// <summary>Separacion longitudinal entre barras (mm).</summary>
    public double LongitudinalSpacingMm { get; set; } = 150;

    /// <summary>Separacion transversal (mm).</summary>
    public double TransverseSpacingMm { get; set; } = 150;

    /// <summary>Longitud del panel en direccion longitudinal (m).</summary>
    public double PanelLengthM { get; set; } = 6.00;

    /// <summary>Ancho del panel en direccion transversal (m).</summary>
    public double PanelWidthM { get; set; } = 2.20;

    /// <summary>Designacion del acero (normalmente B500T para mallazo).</summary>
    public string SteelGrade { get; set; } = "B500T";

    /// <summary>Numero de paneles identicos.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Punto de referencia en planta (esquina inferior izquierda).</summary>
    public Point3D InsertionPoint { get; set; } = Point3D.Origin;

    /// <summary>Rotacion del panel sobre Z (grados).</summary>
    public double RotationDegrees { get; set; }

    public Guid OwnerElementId { get; set; }

    /// <summary>Area de acero por metro en direccion longitudinal (mm^2/m).</summary>
    public double AsLongitudinal_mm2_per_m
        => 1000.0 * (Math.PI * LongitudinalDiameterMm * LongitudinalDiameterMm / 4.0) / LongitudinalSpacingMm;

    /// <summary>Area de acero por metro en direccion transversal (mm^2/m).</summary>
    public double AsTransverse_mm2_per_m
        => 1000.0 * (Math.PI * TransverseDiameterMm * TransverseDiameterMm / 4.0) / TransverseSpacingMm;

    public double PanelAreaM2 => PanelLengthM * PanelWidthM;

    /// <summary>Peso total de los paneles (kg).</summary>
    public double TotalWeightKg
    {
        get
        {
            double barsLong = Math.Floor(PanelWidthM * 1000.0 / LongitudinalSpacingMm) + 1;
            double barsTrans = Math.Floor(PanelLengthM * 1000.0 / TransverseSpacingMm) + 1;
            double wLong = 0.00617 * LongitudinalDiameterMm * LongitudinalDiameterMm;
            double wTrans = 0.00617 * TransverseDiameterMm * TransverseDiameterMm;
            double totalPerPanel = barsLong * PanelLengthM * wLong + barsTrans * PanelWidthM * wTrans;
            return totalPerPanel * Quantity;
        }
    }
}
