using CadZapatas.Core.Primitives;

namespace CadZapatas.Reinforcement;

/// <summary>
/// Estribo (cerco) o horquilla. Armadura transversal cerrada para cortante, torsion,
/// confinamiento (pilares) o punzonamiento.
/// Codigo Estructural RD 470/2021 art. 44 (cortante) y art. 58.4 (disposiciones constructivas).
/// </summary>
public class Stirrup
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Mark { get; set; } = string.Empty;

    public StirrupShape Shape { get; set; } = StirrupShape.Rectangular;

    public int DiameterMm { get; set; } = 8;
    public string SteelGrade { get; set; } = "B500SD";

    /// <summary>Separacion entre estribos a lo largo del elemento (m).</summary>
    public double SpacingM { get; set; } = 0.20;

    /// <summary>Numero de ramas activas en cortante (2 para cerco simple, 4 si hay interior).</summary>
    public int NumberOfLegs { get; set; } = 2;

    /// <summary>Dimensiones exteriores del cerco rectangular (m).</summary>
    public double OuterWidthM { get; set; } = 0.30;
    public double OuterHeightM { get; set; } = 0.40;

    /// <summary>Diametro exterior para cerco helicoidal / circular (m).</summary>
    public double OuterDiameterM { get; set; } = 0.30;

    /// <summary>Paso del helicoidal (m). Solo aplica en shape == Helical.</summary>
    public double HelicalPitchM { get; set; } = 0.10;

    /// <summary>Gancho extremo (obligatorio 135 grados en zonas sismicas per CE 58.4.3).</summary>
    public RebarHookType HookType { get; set; } = RebarHookType.Standard135;

    public Guid OwnerElementId { get; set; }

    /// <summary>Longitud total de la region armada con este estribo (m).</summary>
    public double RegionLengthM { get; set; }

    /// <summary>Numero de estribos necesarios = floor(RegionLengthM / SpacingM) + 1.</summary>
    public int Count => (int)Math.Floor(RegionLengthM / SpacingM) + 1;

    /// <summary>Longitud desarrollada de un estribo (m), incluyendo patillas.</summary>
    public double DevelopedLengthPerStirrupM
    {
        get
        {
            double baseLen = Shape switch
            {
                StirrupShape.Rectangular => 2 * (OuterWidthM + OuterHeightM),
                StirrupShape.Square => 4 * OuterWidthM,
                StirrupShape.Circular => Math.PI * OuterDiameterM,
                StirrupShape.Helical => Math.PI * OuterDiameterM,
                _ => 2 * (OuterWidthM + OuterHeightM)
            };
            // Patillas 10Ø (135 grados, CE 58.4.3) por cada extremo
            double hookExtension = HookType == RebarHookType.None
                ? 0.0
                : 2 * 10.0 * DiameterMm / 1000.0;
            return baseLen + hookExtension;
        }
    }

    /// <summary>Longitud total de acero de estribos en esta region (m).</summary>
    public double TotalSteelLengthM => Count * DevelopedLengthPerStirrupM;

    /// <summary>Peso total de acero de estribos en esta region (kg).</summary>
    public double TotalWeightKg => TotalSteelLengthM * 0.00617 * DiameterMm * DiameterMm;

    /// <summary>Area de acero transversal por metro (mm^2/m), usada en comprobaciones de cortante.</summary>
    public double Asw_per_s_mm2_per_m
    {
        get
        {
            double areaPerLeg = Math.PI * DiameterMm * DiameterMm / 4.0;
            return NumberOfLegs * areaPerLeg / SpacingM;
        }
    }
}

public enum StirrupShape
{
    Rectangular,
    Square,
    Circular,
    Helical,                // zuncho helicoidal continuo
    OpenU,                  // horquilla abierta en U
    MultipleBranches        // cerco con ramas interiores adicionales
}
