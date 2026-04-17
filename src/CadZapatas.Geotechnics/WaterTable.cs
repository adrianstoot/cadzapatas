using CadZapatas.Core.Bim;

namespace CadZapatas.Geotechnics;

/// <summary>
/// Nivel freatico. Tipo estatico, estacional, artesiano o colgado.
/// </summary>
public class WaterTable : BimObject
{
    public override string ObjectType => "WaterTable";
    public WaterTableKind Kind { get; set; } = WaterTableKind.Static;
    public double Elevation { get; set; }
    public double? SeasonalHigh { get; set; }
    public double? SeasonalLow { get; set; }
    public DateTime? MeasurementDate { get; set; }

    // Agresividad quimica para durabilidad y seleccion de clase de exposicion
    public double? SulfateContentMgPerL { get; set; }
    public double? pH { get; set; }
    public double? AggressiveCO2MgPerL { get; set; }
}

public enum WaterTableKind { Static, Seasonal, Artesian, Perched }
