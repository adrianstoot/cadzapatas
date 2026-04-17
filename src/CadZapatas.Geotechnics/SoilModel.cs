using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;

namespace CadZapatas.Geotechnics;

/// <summary>
/// Modelo geotecnico del proyecto. Agrupa estratos, sondeos y nivel freatico.
/// </summary>
public class SoilModel : BimObject
{
    public override string ObjectType => "SoilModel";
    public List<SoilLayer> Layers { get; set; } = new();
    public List<Borehole> Boreholes { get; set; } = new();
    public List<WaterTable> WaterTables { get; set; } = new();

    public SoilLayer? LayerAtElevation(double elevation)
    {
        foreach (var l in Layers)
        {
            double top = Math.Max(l.TopElevation, l.BottomElevation);
            double bot = Math.Min(l.TopElevation, l.BottomElevation);
            if (elevation <= top + 1e-9 && elevation >= bot - 1e-9) return l;
        }
        return null;
    }

    public WaterTable? DesignWaterTable(string scenario = "SeasonalHigh")
    {
        if (WaterTables.Count == 0) return null;
        // Toma el nivel freatico mas alto por defecto (caso pesimo para empujes y flotacion).
        return WaterTables.OrderByDescending(w => w.SeasonalHigh ?? w.Elevation).First();
    }

    /// <summary>
    /// Devuelve la tension vertical efectiva a una elevacion dada, integrando pesos especificos
    /// naturales por encima del freatico y sumergidos por debajo.
    /// </summary>
    public double EffectiveVerticalStress_Pa(double elevation, double groundElevation)
    {
        if (elevation >= groundElevation) return 0;
        double sigma = 0;
        double z = groundElevation;
        double waterLevel = DesignWaterTable()?.Elevation ?? double.NegativeInfinity;

        while (z > elevation)
        {
            double zNext = Math.Max(elevation, z - 0.1); // integracion simple por capas de 10 cm
            var layer = LayerAtElevation((z + zNext) / 2);
            if (layer == null) { z = zNext; continue; }
            double gamma = (z + zNext) / 2 < waterLevel
                ? layer.Parameters.SubmergedUnitWeightN_Per_M3
                : layer.Parameters.UnitWeight.CharacteristicValue;
            sigma += gamma * (z - zNext);
            z = zNext;
        }
        // Resta de subpresion por debajo del freatico: ya contemplado usando gamma sumergido en integracion.
        return sigma;
    }
}
