namespace CadZapatas.NormativeEngine;

/// <summary>
/// Contexto de ejecucion de una regla. El orchestrator rellena el diccionario de inputs
/// (y opcionalmente referencias a objetos BIM del proyecto) antes de invocar al evaluator.
/// </summary>
public class RuleContext
{
    public required Guid ProjectId { get; init; }
    public required Guid ElementId { get; init; }
    public string ElementType { get; init; } = string.Empty;

    public Dictionary<string, object> Inputs { get; init; } = new();

    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Factor de seguridad global a aplicar si la norma lo permite parametrizar.</summary>
    public double GlobalSafetyFactor { get; init; } = 1.0;

    public T GetInput<T>(string key)
    {
        if (!Inputs.TryGetValue(key, out var raw))
            throw new KeyNotFoundException($"Input '{key}' no proporcionado al evaluar la regla.");
        if (raw is T typed) return typed;
        try { return (T)Convert.ChangeType(raw, typeof(T))!; }
        catch { throw new InvalidCastException($"Input '{key}' no convertible a {typeof(T).Name}."); }
    }

    public bool TryGetInput<T>(string key, out T value)
    {
        value = default!;
        if (!Inputs.TryGetValue(key, out var raw)) return false;
        if (raw is T t) { value = t; return true; }
        try { value = (T)Convert.ChangeType(raw, typeof(T))!; return true; }
        catch { return false; }
    }
}
