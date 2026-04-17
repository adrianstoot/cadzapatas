using CadZapatas.Core.Audit;

namespace CadZapatas.NormativeEngine;

/// <summary>
/// Registro de reglas normativas estandar del catalogo base.
/// Las reglas de calculo mas complejas (hundimiento, deslizamiento, vuelco, punzonamiento)
/// se registran desde el modulo CadZapatas.Calculation. Aqui solo reglas de disposiciones
/// constructivas y comprobaciones directas que no requieren el modulo de calculo.
/// </summary>
public static class StandardRuleRegistrations
{
    public static void RegisterAll(RuleCatalog catalog)
    {
        RegisterGeometricRules(catalog);
        RegisterDurabilityRules(catalog);
    }

    private static void RegisterGeometricRules(RuleCatalog catalog)
    {
        // --- CTE DB-SE-C 4.2.1: canto minimo de zapata (SUPOSICION DE DISENO: h >= 0.25 m) ---
        catalog.Register(new NormRule
        {
            RuleId = "CTE.DB_SE_C.4.2.1.CantoMinimoZapata",
            Title = "Canto minimo de zapata",
            Norm = new NormReference
            {
                Code = "CTE DB-SE-C",
                Version = "2019",
                Article = "4.2.1",
                TitleEs = "Dimensiones minimas"
            },
            Kind = CheckKind.Geometric,
            FormulaLatex = "h \\geq h_{min}",
            AppliesToObjectTypes = new() { "IsolatedFooting", "StripFooting" },
            Inputs =
            {
                new RuleInput { Name = "thickness_m", ValueType = typeof(double), Units = "m" }
            },
            Evaluator = ctx =>
            {
                double h = ctx.GetInput<double>("thickness_m");
                double hMin = 0.25;
                return new CalcTrace
                {
                    Inputs = new()
                    {
                        new CalcVariable { Symbol = "h", Value = h, Units = "m",
                                           Description = "canto de la zapata" }
                    },
                    FormulaLatex = "h \\geq 0.25 \\; m",
                    Result = new CalcVariable { Symbol = "h", Value = h, Units = "m" },
                    Limit = new CalcVariable { Symbol = "h_{min}", Value = hMin, Units = "m" },
                    Utilization = hMin / h,
                    Verdict = h >= hMin ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
                    Message = h >= hMin ? "Canto suficiente." : $"Canto inferior al minimo ({hMin} m)."
                };
            }
        });

        // --- CE 55.1 separacion minima barras ---
        catalog.Register(new NormRule
        {
            RuleId = "CE.55.1.SeparacionMinimaBarras",
            Title = "Separacion libre minima entre barras",
            Norm = new NormReference
            {
                Code = "Codigo Estructural",
                Version = "RD 470/2021",
                Article = "55.1",
                TitleEs = "Disposicion de armaduras"
            },
            Kind = CheckKind.Constructive,
            FormulaLatex = "s \\geq \\max(\\phi, 20\\,mm, 1.2\\,d_g)",
            AppliesToObjectTypes = new() { "IsolatedFooting", "StripFooting", "MatFoundation",
                                            "RetainingWall", "PileCap" },
            Inputs =
            {
                new RuleInput { Name = "clear_spacing_mm", ValueType = typeof(double), Units = "mm" },
                new RuleInput { Name = "bar_diameter_mm", ValueType = typeof(int), Units = "mm" },
                new RuleInput { Name = "aggregate_max_mm", ValueType = typeof(double), Units = "mm",
                                Required = false }
            },
            Evaluator = ctx =>
            {
                double s = ctx.GetInput<double>("clear_spacing_mm");
                int phi = ctx.GetInput<int>("bar_diameter_mm");
                double dg = ctx.TryGetInput<double>("aggregate_max_mm", out var v) ? v : 20.0;
                double smin = Math.Max(phi, Math.Max(20.0, 1.2 * dg));
                return new CalcTrace
                {
                    Inputs = new()
                    {
                        new CalcVariable { Symbol = "s", Value = s, Units = "mm" },
                        new CalcVariable { Symbol = "\\phi", Value = phi, Units = "mm" },
                        new CalcVariable { Symbol = "d_g", Value = dg, Units = "mm" }
                    },
                    FormulaLatex = "s_{min} = \\max(\\phi, 20, 1.2 d_g)",
                    Result = new CalcVariable { Symbol = "s", Value = s, Units = "mm" },
                    Limit = new CalcVariable { Symbol = "s_{min}", Value = smin, Units = "mm" },
                    Utilization = smin / s,
                    Verdict = s >= smin ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
                    Message = s >= smin
                        ? $"Separacion {s:F0} mm >= {smin:F0} mm."
                        : $"Separacion insuficiente ({s:F0} < {smin:F0} mm)."
                };
            }
        });
    }

    private static void RegisterDurabilityRules(RuleCatalog catalog)
    {
        // --- CE 37.2.4 recubrimiento nominal ---
        catalog.Register(new NormRule
        {
            RuleId = "CE.37.2.4.RecubrimientoNominal",
            Title = "Recubrimiento nominal por durabilidad",
            Norm = new NormReference
            {
                Code = "Codigo Estructural",
                Version = "RD 470/2021",
                Article = "37.2.4",
                TitleEs = "Recubrimientos"
            },
            Kind = CheckKind.Durability,
            FormulaLatex = "c_{nom} = c_{min} + \\Delta c",
            AppliesToObjectTypes = new() { "IsolatedFooting", "StripFooting", "MatFoundation",
                                            "RetainingWall", "PileCap", "Pile", "DiaphragmWall" },
            Inputs =
            {
                new RuleInput { Name = "nominal_cover_mm", ValueType = typeof(double), Units = "mm" },
                new RuleInput { Name = "required_cover_mm", ValueType = typeof(double), Units = "mm" }
            },
            Evaluator = ctx =>
            {
                double cnom = ctx.GetInput<double>("nominal_cover_mm");
                double creq = ctx.GetInput<double>("required_cover_mm");
                return new CalcTrace
                {
                    Inputs = new()
                    {
                        new CalcVariable { Symbol = "c_{nom}", Value = cnom, Units = "mm" },
                        new CalcVariable { Symbol = "c_{req}", Value = creq, Units = "mm" }
                    },
                    FormulaLatex = "c_{nom} \\geq c_{min} + \\Delta c",
                    Result = new CalcVariable { Symbol = "c_{nom}", Value = cnom, Units = "mm" },
                    Limit = new CalcVariable { Symbol = "c_{req}", Value = creq, Units = "mm" },
                    Utilization = creq / cnom,
                    Verdict = cnom >= creq ? CheckVerdictCode.Pass : CheckVerdictCode.Fail,
                    Message = cnom >= creq
                        ? "Recubrimiento suficiente para la clase de exposicion."
                        : $"Recubrimiento insuficiente ({cnom} < {creq} mm)."
                };
            }
        });
    }
}
