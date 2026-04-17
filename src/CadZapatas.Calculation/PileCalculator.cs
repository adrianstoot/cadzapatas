using CadZapatas.Core.Audit;
using CadZapatas.Foundations;
using CadZapatas.Geotechnics;

namespace CadZapatas.Calculation;

/// <summary>
/// Calculo de capacidad portante de pilotes por suma de fuste + punta (metodo estatico).
/// Q_u = Q_p + Q_s = q_p * A_p + sumatorio (f_s_i * As_i)
/// Referencia: CTE DB-SE-C 5.3 (pilotes) y correlaciones clasicas.
/// </summary>
public class PileCalculator
{
    private static double D(TrackedParameter p) => p.DesignValue ?? p.CharacteristicValue;

    /// <summary>
    /// Resistencia por punta (formula de Meyerhof para arenas): q_p = N60 * sigma_atm
    /// (con N60 del estrato de punta). Para cohesivos q_p = 9 * c_u.
    /// </summary>
    public static double TipResistancePa(SoilLayer tipLayer)
    {
        double phi = D(tipLayer.Parameters.FrictionAngleDegrees);
        double c = D(tipLayer.Parameters.CohesionEffective);
        double cu = D(tipLayer.Parameters.CohesionUndrained);
        if (cu > 1000) return 9.0 * cu;                         // suelos cohesivos
        if (phi > 25) return D(tipLayer.Parameters.SptNCharacteristic) * 100000.0;  // arenas
        // Mixto: combinar
        return 9.0 * c + phi * 10000.0;
    }

    /// <summary>Friccion lateral en un estrato granular: f_s = K * sigma'_v * tan(delta).</summary>
    public static double ShaftFrictionPa(SoilLayer layer, double effectiveStressVertical_Pa)
    {
        double phi = D(layer.Parameters.FrictionAngleDegrees);
        double cu = D(layer.Parameters.CohesionUndrained);
        if (cu > 1000)
        {
            double alpha = cu < 25000 ? 1.0 : (cu < 75000 ? 0.5 : 0.3);
            return alpha * cu;              // metodo alpha
        }
        double K = 1.0 - Math.Sin(phi * Math.PI / 180.0);       // K0
        double delta = 2.0 / 3.0 * phi * Math.PI / 180.0;
        return K * effectiveStressVertical_Pa * Math.Tan(delta);
    }

    /// <summary>
    /// Ejecuta el calculo completo para un pilote individual en el modelo de suelo.
    /// Devuelve una traza con Q_p, Q_s y capacidad de diseno Q_Rd = Q_u / F (F=3).
    /// </summary>
    public CalcTrace Run(Pile pile, SoilModel soilModel)
    {
        double tipElev = pile.TipElevation;
        double headElev = pile.HeadElevation;
        var tipLayer = soilModel.LayerAtElevation(tipElev) ??
                       (soilModel.Layers.Count > 0 ? soilModel.Layers[^1] : null);

        double Ap = Math.PI * pile.Diameter * pile.Diameter / 4.0;
        double Qp = tipLayer != null ? TipResistancePa(tipLayer) * Ap : 0.0;

        // Integra friccion lateral estrato a estrato
        double Qs = 0.0;
        double zTop = headElev;
        foreach (var layer in soilModel.Layers)
        {
            if (layer.BottomElevation >= headElev) continue;
            if (layer.TopElevation <= tipElev) break;
            double zt = Math.Min(layer.TopElevation, headElev);
            double zb = Math.Max(layer.BottomElevation, tipElev);
            double thick = Math.Abs(zt - zb);
            double sigmaV = soilModel.EffectiveVerticalStress_Pa((zt + zb) / 2.0, headElev);
            double fs = ShaftFrictionPa(layer, sigmaV);
            double As = Math.PI * pile.Diameter * thick;
            Qs += fs * As;
        }

        double Qu = Qp + Qs;
        double Qd = Qu / 3.0;
        double Qed = pile.DesignActions.N_kN * 1000.0;

        return new CalcTrace
        {
            ElementId = pile.Id,
            ElementType = pile.ObjectType,
            CheckId = "CTE.DB_SE_C.5.3.HundimientoPilote",
            CheckName = "Capacidad portante del pilote",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "5.3",
                Title = "Resistencia por fuste y punta"
            },
            FormulaLatex = "Q_u = q_p A_p + \\sum f_{s,i} A_{s,i}",
            Inputs = new()
            {
                new CalcVariable { Symbol = "D", Value = pile.Diameter, Unit = "m" },
                new CalcVariable { Symbol = "L", Value = pile.Length, Unit = "m" },
                new CalcVariable { Symbol = "Q_p", Value = Qp/1000.0, Unit = "kN" },
                new CalcVariable { Symbol = "Q_s", Value = Qs/1000.0, Unit = "kN" }
            },
            Result = new CalcVariable { Symbol = "Q_{Ed}", Value = Qed/1000.0, Unit = "kN" },
            Limit = new CalcVariable { Symbol = "Q_{Rd}", Value = Qd/1000.0, Unit = "kN" },
            Utilization = Qed / Math.Max(Qd, 1e-6),
            Verdict = Qed <= Qd ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = Qed <= Qd
                ? $"Pilote OK: Q_Ed={Qed/1000:F0} kN <= Q_Rd={Qd/1000:F0} kN."
                : $"Pilote insuficiente (Q_Ed={Qed/1000:F0} > Q_Rd={Qd/1000:F0} kN). Aumentar L o Ø."
        };
    }
}
