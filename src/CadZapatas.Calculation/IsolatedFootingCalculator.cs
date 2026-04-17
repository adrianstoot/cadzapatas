using CadZapatas.Core.Audit;
using CadZapatas.Foundations;
using CadZapatas.Geotechnics;
using CadZapatas.Materials;

namespace CadZapatas.Calculation;

/// <summary>
/// Orquestador de comprobaciones ELU/ELS para una zapata aislada. Enlaza geometria,
/// acciones, suelo de apoyo y clase de hormigon/acero. Genera una lista de CalcTrace
/// auditables para la memoria de calculo.
///
/// Unidades internas: N, m, Pa, grados (para angulos en argumentos), rad internamente.
/// </summary>
public class IsolatedFootingCalculator
{
    private static double D(TrackedParameter p) => p.DesignValue ?? p.CharacteristicValue;

    public List<CalcTrace> Run(IsolatedFooting footing, SoilParameterSet soil,
                                ConcreteMaterial concrete, RebarSteelMaterial steel)
    {
        var traces = new List<CalcTrace>();

        double N = footing.DesignActions.N_kN * 1000.0;       // kN -> N
        double Vx = footing.DesignActions.Vx_kN * 1000.0;
        double Vy = footing.DesignActions.Vy_kN * 1000.0;
        double Mx = footing.DesignActions.Mx_kNm * 1000.0;    // kNm -> N*m
        double My = footing.DesignActions.My_kNm * 1000.0;

        double B = Math.Min(footing.Length, footing.Width);
        double L = Math.Max(footing.Length, footing.Width);
        double H = footing.Thickness;
        double d = H - footing.NominalCover;                   // canto util (m)

        // Peso propio de la zapata (hormigon 25 kN/m3)
        double weightFoot = footing.VolumeConcrete * 25000.0;   // N
        double Ntotal = N + weightFoot;

        double phi = D(soil.FrictionAngleDegrees);            // deg
        double cPa = D(soil.CohesionEffective);               // Pa
        double gammaN = D(soil.UnitWeight);                   // N/m3 si esta correctamente parametrizado
        double Demb = Math.Max(0.5, H);                        // SUPOSICION DE DISENO si no hay dato D

        // --- 1. Hundimiento (Brinch-Hansen) ---------------------------------
        var inputs = new BearingCapacityInputs
        {
            B = B, L = L,
            EmbedmentDepth = Demb,
            PhiDeg = phi,
            CohesionPa = cPa,
            EffectiveUnitWeight = gammaN,
            OverburdenPressurePa = gammaN * Demb,
            VerticalLoad = Ntotal,
            HorizontalLoad = Math.Sqrt(Vx * Vx + Vy * Vy),
            EccentricityB = Math.Abs(My / Math.Max(Ntotal, 1)),
            EccentricityL = Math.Abs(Mx / Math.Max(Ntotal, 1))
        };
        double qu = BearingCapacity.UltimatePressurePa(inputs);
        double qd = qu / 3.0;    // F=3 metodo global (SUPOSICION DE DISENO)
        (double qEd, _) = StabilityChecks.ContactPressure(Ntotal, Mx, My, B, L);
        traces.Add(new CalcTrace
        {
            ElementId = footing.Id,
            ElementType = footing.ObjectType,
            CheckId = "CTE.DB_SE_C.4.3.Hundimiento",
            CheckName = "Hundimiento (Brinch-Hansen)",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.3.3",
                Title = "Presion de hundimiento (formulacion analitica)"
            },
            FormulaLatex = "q_u = c N_c s_c d_c i_c + q_0 N_q s_q d_q i_q + 0.5 \\gamma B' N_\\gamma s_\\gamma d_\\gamma i_\\gamma",
            Inputs = new()
            {
                new CalcVariable { Symbol = "\\phi'", Value = phi, Unit = "deg" },
                new CalcVariable { Symbol = "c'", Value = cPa / 1000.0, Unit = "kPa" },
                new CalcVariable { Symbol = "\\gamma", Value = gammaN / 1000.0, Unit = "kN/m3" },
                new CalcVariable { Symbol = "B'", Value = B - 2 * inputs.EccentricityB, Unit = "m" },
                new CalcVariable { Symbol = "N_d", Value = Ntotal / 1000.0, Unit = "kN" }
            },
            Result = new CalcVariable { Symbol = "q_{Ed}", Value = qEd / 1000.0, Unit = "kPa" },
            Limit = new CalcVariable { Symbol = "q_{Rd}", Value = qd / 1000.0, Unit = "kPa" },
            Utilization = qEd / Math.Max(qd, 1e-6),
            Verdict = qEd <= qd ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = qEd <= qd
                ? $"Presion transmitida {qEd/1000:F1} kPa < q_adm {qd/1000:F1} kPa."
                : $"Hundimiento: q_Ed {qEd/1000:F1} > q_adm {qd/1000:F1} kPa. Ampliar la zapata o mejorar apoyo."
        });

        // --- 2. Deslizamiento -----------------------------------------------
        double H_horiz = inputs.HorizontalLoad;
        double delta = 2.0 / 3.0 * phi;
        double cs = StabilityChecks.SlidingSafetyFactor(Ntotal, H_horiz, delta,
                                                         0.5 * cPa, B * L);
        traces.Add(new CalcTrace
        {
            ElementId = footing.Id,
            ElementType = footing.ObjectType,
            CheckId = "CTE.DB_SE_C.4.3.Deslizamiento",
            CheckName = "Deslizamiento",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.3.2",
                Title = "Comprobacion al deslizamiento"
            },
            FormulaLatex = "CS = \\frac{V \\tan \\delta + c_a A + E_p}{H}",
            Inputs = new()
            {
                new CalcVariable { Symbol = "V", Value = Ntotal / 1000.0, Unit = "kN" },
                new CalcVariable { Symbol = "H", Value = H_horiz / 1000.0, Unit = "kN" },
                new CalcVariable { Symbol = "\\delta", Value = delta, Unit = "deg" }
            },
            Result = new CalcVariable { Symbol = "CS", Value = cs, Unit = "-" },
            Limit = new CalcVariable { Symbol = "CS_{min}", Value = 1.5, Unit = "-" },
            Utilization = 1.5 / Math.Max(cs, 1e-6),
            Verdict = cs >= 1.5 ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = cs >= 1.5
                ? $"CS_deslizamiento = {cs:F2} >= 1.5."
                : $"Deslizamiento insuficiente (CS={cs:F2} < 1.5). Aumentar zapata o colocar tacon."
        });

        // --- 3. Vuelco -------------------------------------------------------
        double mStab = Ntotal * B / 2.0;
        double mOver = Math.Abs(Mx) + Math.Abs(My) + Math.Max(Math.Abs(Vx), Math.Abs(Vy)) * H;
        double csVuelco = StabilityChecks.OverturningSafetyFactor(mStab, mOver);
        traces.Add(new CalcTrace
        {
            ElementId = footing.Id,
            ElementType = footing.ObjectType,
            CheckId = "CTE.DB_SE_C.4.3.Vuelco",
            CheckName = "Vuelco",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C", Version = "2019", Article = "4.3.2",
                Title = "Comprobacion al vuelco"
            },
            FormulaLatex = "CS = M_{est} / M_{volc}",
            Result = new CalcVariable { Symbol = "CS", Value = csVuelco, Unit = "-" },
            Limit = new CalcVariable { Symbol = "CS_{min}", Value = 2.0, Unit = "-" },
            Utilization = 2.0 / Math.Max(csVuelco, 1e-6),
            Verdict = csVuelco >= 2.0 ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = csVuelco >= 2.0
                ? $"CS_vuelco = {csVuelco:F2} >= 2.0."
                : $"Vuelco insuficiente (CS={csVuelco:F2} < 2.0)."
        });

        // --- 4. Flexion (momento en la seccion critica a d/2 del borde del pilar) ---
        double volado = (B - footing.ColumnLengthX) / 2.0;
        double q = Ntotal / (B * L);
        double md = 0.5 * q * volado * volado * L;         // N*m en la direccion B
        double asReq = ConcreteSectionChecks.RequiredTensionAreaM2(md, L, d,
                                                                    concrete.Fck_MPa * 1e6,
                                                                    steel.Fyk_MPa * 1e6);
        double asMin = ConcreteSectionChecks.MinimumTensionAreaM2(L, H, steel.Grade);
        double asAdoptada = Math.Max(asReq, asMin);
        traces.Add(new CalcTrace
        {
            ElementId = footing.Id,
            ElementType = footing.ObjectType,
            CheckId = "CE.42.Flexion.Zapata",
            CheckName = "Armadura a flexion",
            Norm = new NormReference
            {
                Code = "Codigo Estructural", Version = "RD 470/2021", Article = "42",
                Title = "Estado limite ultimo de flexion"
            },
            FormulaLatex = "A_s = \\omega \\cdot b \\cdot d \\cdot f_{cd}/f_{yd}",
            Inputs = new()
            {
                new CalcVariable { Symbol = "M_d", Value = md / 1000.0, Unit = "kNm" },
                new CalcVariable { Symbol = "b", Value = L, Unit = "m" },
                new CalcVariable { Symbol = "d", Value = d, Unit = "m" },
                new CalcVariable { Symbol = "f_{ck}", Value = concrete.Fck_MPa, Unit = "MPa" },
                new CalcVariable { Symbol = "f_{yk}", Value = steel.Fyk_MPa, Unit = "MPa" }
            },
            Result = new CalcVariable { Symbol = "A_{s,req}", Value = asReq * 1e4, Unit = "cm2" },
            Limit = new CalcVariable { Symbol = "A_{s,min}", Value = asMin * 1e4, Unit = "cm2" },
            Utilization = 1.0,   // siempre adoptable
            Verdict = CheckVerdictCode.Pass,
            Message = $"Armadura requerida {asReq*1e4:F2} cm2; minima {asMin*1e4:F2} cm2. Adoptar {asAdoptada*1e4:F2} cm2."
        });

        // --- 5. Punzonamiento ------------------------------------------------
        double perim = 2.0 * (footing.ColumnLengthX + footing.ColumnLengthY);
        double vRd = ConcreteSectionChecks.PunchingCapacityN(perim, d, concrete.Fck_MPa);
        double vEd = Ntotal;
        traces.Add(new CalcTrace
        {
            ElementId = footing.Id,
            ElementType = footing.ObjectType,
            CheckId = "CE.45.Punzonamiento",
            CheckName = "Punzonamiento",
            Norm = new NormReference
            {
                Code = "Codigo Estructural", Version = "RD 470/2021", Article = "45",
                Title = "Estado limite ultimo de punzonamiento"
            },
            FormulaLatex = "V_{Rd,max} = 0.5 \\nu f_{cd} u_1 d",
            Result = new CalcVariable { Symbol = "V_{Ed}", Value = vEd / 1000.0, Unit = "kN" },
            Limit = new CalcVariable { Symbol = "V_{Rd,max}", Value = vRd / 1000.0, Unit = "kN" },
            Utilization = vEd / Math.Max(vRd, 1),
            Verdict = vEd <= vRd ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
            Message = vEd <= vRd
                ? "Punzonamiento: cumple."
                : $"Punzonamiento excede capacidad (V_Ed={vEd/1000:F0} > V_Rd,max={vRd/1000:F0} kN). Aumentar canto."
        });

        return traces;
    }
}
