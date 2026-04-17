using CadZapatas.Core.Audit;

namespace CadZapatas.NormativeEngine;

/// <summary>
/// Regla normativa declarativa. Enlaza un identificador unico con su referencia a la norma,
/// inputs requeridos, formula simbolica y el evaluador que produce el CalcTrace.
/// El motor carga las reglas desde catalogos versionados (CTE 2019, CE 2021, EHE-08 legacy).
/// </summary>
public class NormRule
{
    /// <summary>Identificador unico (ej. "CTE.DB_SE_C.4.3.1.HundimientoBrinchHansen").</summary>
    public required string RuleId { get; init; }

    /// <summary>Titulo legible para el usuario.</summary>
    public required string Title { get; init; }

    /// <summary>Norma referenciada (incluye version).</summary>
    public required NormReference Norm { get; init; }

    /// <summary>Entradas requeridas (nombre -> tipo esperado).</summary>
    public List<RuleInput> Inputs { get; init; } = new();

    /// <summary>Formula en forma simbolica o LaTeX para la trazabilidad.</summary>
    public string FormulaLatex { get; init; } = string.Empty;

    /// <summary>Tipo de limite (ULS, SLS, geometrico, constructivo).</summary>
    public CheckKind Kind { get; init; } = CheckKind.ULS;

    /// <summary>Funcion evaluadora. Recibe el contexto con los inputs y devuelve el CalcTrace completo.</summary>
    public required Func<RuleContext, CalcTrace> Evaluator { get; init; }

    /// <summary>Aplicabilidad: tipos de elementos a los que esta regla puede aplicarse.</summary>
    public List<string> AppliesToObjectTypes { get; init; } = new();

    /// <summary>Fecha de entrada en vigor y fecha de derogacion si la hay.</summary>
    public DateTime EffectiveFrom { get; init; } = new(2019, 12, 1);
    public DateTime? DerogatedOn { get; init; }

    public bool IsEffectiveAt(DateTime date)
        => date >= EffectiveFrom && (DerogatedOn is null || date < DerogatedOn.Value);
}

public class RuleInput
{
    public required string Name { get; init; }
    public required Type ValueType { get; init; }
    public string? Units { get; init; }
    public bool Required { get; init; } = true;
    public string? Description { get; init; }
}

public enum CheckKind
{
    ULS,            // estado limite ultimo (resistencia, estabilidad)
    SLS,            // estado limite de servicio (fisuracion, deformacion)
    Geometric,      // restriccion geometrica (solapes, recubrimientos)
    Constructive,   // disposicion constructiva (diametros, separaciones)
    Durability,     // durabilidad (recubrimiento, exposicion)
    Executability   // ejecucion (radios de doblado, tolerancias)
}
