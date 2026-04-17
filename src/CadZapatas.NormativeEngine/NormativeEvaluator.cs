using CadZapatas.Core.Audit;

namespace CadZapatas.NormativeEngine;

/// <summary>
/// Evaluador de reglas normativas. Orquesta la ejecucion de una o varias reglas sobre un contexto
/// y consolida los CalcTrace resultantes. Cada ejecucion es independiente, trazable y auditable.
/// </summary>
public class NormativeEvaluator
{
    private readonly RuleCatalog _catalog;

    public NormativeEvaluator(RuleCatalog catalog)
    {
        _catalog = catalog;
    }

    /// <summary>Evalua una regla concreta por su identificador.</summary>
    public CalcTrace Evaluate(string ruleId, RuleContext ctx)
    {
        var rule = _catalog.Get(ruleId)
            ?? throw new InvalidOperationException($"Regla '{ruleId}' no encontrada en el catalogo.");

        if (!rule.IsEffectiveAt(ctx.EvaluatedAt))
            throw new InvalidOperationException(
                $"Regla '{ruleId}' no esta vigente en {ctx.EvaluatedAt:yyyy-MM-dd}.");

        ValidateInputs(rule, ctx);

        try
        {
            var trace = rule.Evaluator(ctx);
            trace.Norm = rule.Norm;
            trace.CheckId = rule.RuleId;
            trace.EvaluatedAt = ctx.EvaluatedAt;
            return trace;
        }
        catch (Exception ex)
        {
            return new CalcTrace
            {
                CheckId = rule.RuleId,
                Norm = rule.Norm,
                Verdict = CheckVerdictCode.Error,
                Message = $"Error evaluando regla: {ex.Message}",
                EvaluatedAt = ctx.EvaluatedAt
            };
        }
    }

    /// <summary>Evalua todas las reglas aplicables a un tipo de objeto.</summary>
    public List<CalcTrace> EvaluateAllForType(string objectType, RuleContext ctx)
    {
        var traces = new List<CalcTrace>();
        foreach (var rule in _catalog.GetByObjectType(objectType))
        {
            if (!rule.IsEffectiveAt(ctx.EvaluatedAt)) continue;
            try
            {
                traces.Add(Evaluate(rule.RuleId, ctx));
            }
            catch (Exception ex)
            {
                traces.Add(new CalcTrace
                {
                    CheckId = rule.RuleId,
                    Norm = rule.Norm,
                    Verdict = CheckVerdictCode.Error,
                    Message = $"Error preparando la regla: {ex.Message}"
                });
            }
        }
        return traces;
    }

    private void ValidateInputs(NormRule rule, RuleContext ctx)
    {
        foreach (var req in rule.Inputs.Where(i => i.Required))
        {
            if (!ctx.Inputs.ContainsKey(req.Name))
                throw new InvalidOperationException(
                    $"Input requerido '{req.Name}' ({req.Units ?? req.ValueType.Name}) no suministrado.");
        }
    }
}
