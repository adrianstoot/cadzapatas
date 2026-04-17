using CadZapatas.Core.Audit;
using CadZapatas.Geotechnics;
using CadZapatas.Retaining;

namespace CadZapatas.Calculation;

/// <summary>
/// Orquestador de comprobaciones de un muro de contencion en mensula (cantilever).
/// Hipotesis: trasdos vertical, relleno granular con talud beta, sin nivel freatico
/// (si el muro tiene drenaje). Incluye empuje activo de Coulomb + sobrecarga + agua opcional.
/// Referencias: CTE DB-SE-C cap. 6 (empujes) y 4.6 (contencion).
/// </summary>
public class RetainingWallCalculator
{
    private static double D(TrackedParameter p) => p.DesignValue ?? p.CharacteristicValue;

    public List<CalcTrace> Run(RetainingWall wall, SoilParameterSet backfill, SoilParameterSet foundationSoil,
                                double waterHeightM = 0.0)
    {
        var traces = new List<CalcTrace>();

        double phiB = D(backfill.FrictionAngleDegrees);
        double gammaB = D(backfill.UnitWeight);       // N/m3
        double cB = D(backfill.CohesionEffective);    // Pa

        double phiF = D(foundationSoil.FrictionAngleDegrees);
        double cF = D(foundationSoil.CohesionEffective);
        double gammaF = D(foundationSoil.UnitWeight);

        double H = wall.Height + wall.FoundationThickness;
        double B = wall.BaseWidth;

        double delta = 2.0 / 3.0 * phiB;
        double kA = EarthPressure.KaCoulomb(phiB, delta, 90, wall.BackfillSlopeDegrees);
        double Ea = EarthPressure.ActivePressureResultant(kA, gammaB, H);
        double Eq = EarthPressure.SurchargeResultant(kA, wall.SurchargeKPa * 1000.0, H);
        double Ew = waterHeightM > 0 ? EarthPressure.WaterResultant(waterHeightM) : 0.0;

        double EaTotal = Ea + Eq + Ew;
        // puntos de aplicacion (H/3 para Ea triangular, H/2 para sobrecarga rectangular, Hw/3 para agua)
        double armEa = H / 3.0;
        double armEq = H / 2.0;
        double armEw = waterHeightM / 3.0;
        double momentOverturning = Ea * armEa + Eq * armEq + Ew * armEw;

        // Estabilidad: pesos propios
        double wStem = (wall.StemThicknessTop + wall.StemThicknessBottom) / 2.0 * wall.Height * 25000.0; // N/m
        double wFoot = B * wall.FoundationThickness * 25000.0;
        double wSoilOverHeel = wall.HeelLength * wall.Height * gammaB;   // tierras sobre talon
        double totalV = wStem + wFoot + wSoilOverHeel;
        // brazos estabilizantes al pie de puntera
        double xStem = wall.ToeLength + wall.StemThicknessBottom / 2.0;
        double xFoot = B / 2.0;
        double xSoil = wall.ToeLength + wall.StemThicknessBottom + wall.HeelLength / 2.0;
        double momentStab = wStem * xStem + wFoot * xFoot + wSoilOverHeel * xSoil;

        // --- 1. Vuelco -------------------------------------------------------
        double csVuelco = StabilityChecks.OverturningSafetyFactor(momentStab, momentOverturning);
        traces.Add(new CalcTrace
        {
            ElementId = wall.Id,
            ElementType = wall.ObjectType,
            CheckId = "CTE.DB_SE_C.4.6.Vuelco",
            CheckName = "Vuelco del muro",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.6.2",
                Title = "Estabilidad al vuelco (muros de contencion)"
            },
            FormulaLatex = "CS_{vuelco} = M_{est}/M_{volc} \\geq 2",
            Inputs = new()
            {
                new CalcVariable { Symbol = "\\phi'", Value = phiB, Unit = "deg" },
                new CalcVariable { Symbol = "K_a", Value = kA, Unit = "-" },
                new CalcVariable { Symbol = "H", Value = H, Unit = "m" },
                new CalcVariable { Symbol = "E_a", Value = EaTotal/1000.0, Unit = "kN/m" }
            },
            Result = new CalcVariable { Symbol = "CS", Value = csVuelco, Unit = "-" },
            Limit = new CalcVariable { Symbol = "CS_{min}", Value = 2.0, Unit = "-" },
            Utilization = 2.0 / Math.Max(csVuelco, 1e-6),
            Verdict = csVuelco >= 2.0 ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = csVuelco >= 2.0
                ? $"Muro estable a vuelco (CS={csVuelco:F2})."
                : $"Vuelco insuficiente (CS={csVuelco:F2} < 2). Aumentar talon o canto."
        });

        // --- 2. Deslizamiento -----------------------------------------------
        double deltaBase = 2.0 / 3.0 * phiF;
        double csDesl = StabilityChecks.SlidingSafetyFactor(totalV, EaTotal, deltaBase,
                                                              0.5 * cF, B);
        traces.Add(new CalcTrace
        {
            ElementId = wall.Id,
            ElementType = wall.ObjectType,
            CheckId = "CTE.DB_SE_C.4.6.Deslizamiento",
            CheckName = "Deslizamiento del muro",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.6.2",
                Title = "Estabilidad al deslizamiento"
            },
            FormulaLatex = "CS_{desl} = \\frac{V \\tan\\delta + c_a B}{E_a}",
            Inputs = new()
            {
                new CalcVariable { Symbol = "V", Value = totalV/1000.0, Unit = "kN/m" },
                new CalcVariable { Symbol = "E_a", Value = EaTotal/1000.0, Unit = "kN/m" },
                new CalcVariable { Symbol = "\\delta_{base}", Value = deltaBase, Unit = "deg" }
            },
            Result = new CalcVariable { Symbol = "CS", Value = csDesl, Unit = "-" },
            Limit = new CalcVariable { Symbol = "CS_{min}", Value = 1.5, Unit = "-" },
            Utilization = 1.5 / Math.Max(csDesl, 1e-6),
            Verdict = csDesl >= 1.5 ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = csDesl >= 1.5
                ? $"Deslizamiento: CS={csDesl:F2}."
                : $"Deslizamiento insuficiente (CS={csDesl:F2}). Colocar tacon o alargar cimiento."
        });

        // --- 3. Hundimiento (zapata del muro como corrida) -----------------
        double Mnet = momentStab - momentOverturning;       // respecto al pie de puntera
        double ex = B / 2.0 - Mnet / Math.Max(totalV, 1e-6);
        var inp = new BearingCapacityInputs
        {
            B = B, L = Math.Max(B * 10, 10.0), // por metro lineal
            EmbedmentDepth = wall.FoundationThickness,
            PhiDeg = phiF,
            CohesionPa = cF,
            EffectiveUnitWeight = gammaF,
            OverburdenPressurePa = gammaF * wall.FoundationThickness,
            VerticalLoad = totalV,
            HorizontalLoad = EaTotal,
            EccentricityB = Math.Abs(ex)
        };
        double qu = BearingCapacity.UltimatePressurePa(inp);
        double qAdm = qu / 3.0;
        double qEd = totalV / Math.Max(B - 2 * Math.Abs(ex), 0.1);
        traces.Add(new CalcTrace
        {
            ElementId = wall.Id,
            ElementType = wall.ObjectType,
            CheckId = "CTE.DB_SE_C.4.6.Hundimiento",
            CheckName = "Hundimiento del terreno bajo el muro",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.6.2",
                Title = "Hundimiento (muros)"
            },
            Result = new CalcVariable { Symbol = "q_{Ed}", Value = qEd / 1000.0, Unit = "kPa" },
            Limit = new CalcVariable { Symbol = "q_{adm}", Value = qAdm / 1000.0, Unit = "kPa" },
            Utilization = qEd / Math.Max(qAdm, 1e-6),
            Verdict = qEd <= qAdm ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = qEd <= qAdm
                ? $"q_Ed = {qEd/1000:F0} kPa <= q_adm = {qAdm/1000:F0} kPa."
                : $"Hundimiento: q_Ed = {qEd/1000:F0} > q_adm = {qAdm/1000:F0} kPa."
        });

        return traces;
    }
}
