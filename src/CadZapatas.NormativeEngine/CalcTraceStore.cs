using CadZapatas.Core.Audit;

namespace CadZapatas.NormativeEngine;

/// <summary>
/// Almacen en memoria de trazas de calculo. Agrupa por elemento y por tipo de chequeo.
/// Persistido por el modulo Persistence junto con el proyecto.
/// </summary>
public class CalcTraceStore
{
    private readonly List<CalcTrace> _traces = new();
    private readonly object _lock = new();

    public IReadOnlyList<CalcTrace> All
    {
        get { lock (_lock) return _traces.ToList(); }
    }

    public void Add(CalcTrace trace)
    {
        lock (_lock) _traces.Add(trace);
    }

    public void AddRange(IEnumerable<CalcTrace> traces)
    {
        lock (_lock) _traces.AddRange(traces);
    }

    public IEnumerable<CalcTrace> GetForElement(Guid elementId)
        => All.Where(t => t.ElementId == elementId);

    public IEnumerable<CalcTrace> GetByVerdict(CheckVerdictCode code)
        => All.Where(t => t.Verdict == code);

    public IEnumerable<CalcTrace> GetByRule(string ruleId)
        => All.Where(t => t.CheckId == ruleId);

    public void Clear() { lock (_lock) _traces.Clear(); }

    public void ClearForElement(Guid elementId)
    {
        lock (_lock) _traces.RemoveAll(t => t.ElementId == elementId);
    }

    public int Count { get { lock (_lock) return _traces.Count; } }

    public int CountFails => All.Count(t => t.Verdict == CheckVerdictCode.Fail
                                          || t.Verdict == CheckVerdictCode.Error);
    public int CountWarnings => All.Count(t => t.Verdict == CheckVerdictCode.Warning);
    public int CountPass => All.Count(t => t.Verdict == CheckVerdictCode.Pass);

    /// <summary>
    /// Produce un resumen agregado para la barra de estado / panel de incidencias.
    /// </summary>
    public NormativeSummary Summarize()
    {
        return new NormativeSummary
        {
            TotalChecks = Count,
            Pass = CountPass,
            Warnings = CountWarnings,
            Fails = CountFails,
            MaxUtilization = All.Count == 0 ? 0 : All.Max(t => t.Utilization)
        };
    }
}

public class NormativeSummary
{
    public int TotalChecks { get; set; }
    public int Pass { get; set; }
    public int Warnings { get; set; }
    public int Fails { get; set; }
    public double MaxUtilization { get; set; }
    public bool ProjectIsCompliant => Fails == 0;
}
