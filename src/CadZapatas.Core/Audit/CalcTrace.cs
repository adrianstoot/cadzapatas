namespace CadZapatas.Core.Audit;

/// <summary>
/// Traza completa de una comprobacion normativa. Permite reconstruir el razonamiento
/// del motor: norma, articulo, hipotesis, variables, formula, intermedios, resultado, limite y veredicto.
/// Sirve de base para la memoria de calculo auditable exigida por la practica profesional.
/// </summary>
public class CalcTrace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ElementId { get; set; }
    public string ElementType { get; set; } = string.Empty;

    public NormReference Norm { get; set; } = new();
    public string CheckId { get; set; } = string.Empty;
    public string CheckName { get; set; } = string.Empty;

    public List<Hypothesis> Hypotheses { get; set; } = new();
    public List<CalcVariable> Inputs { get; set; } = new();
    public string? FormulaLatex { get; set; }
    public string? FormulaPlain { get; set; }
    public List<CalcStep> Steps { get; set; } = new();
    public CalcVariable Result { get; set; } = new();
    public CalcVariable Limit { get; set; } = new();

    /// <summary>Ratio R/C o C/R segun naturaleza: &lt;=1.0 cumple.</summary>
    public double Utilization { get; set; }

    public CheckVerdictCode Verdict { get; set; } = CheckVerdictCode.Unknown;

    /// <summary>Mensaje dirigido al usuario (explicacion corta del veredicto).</summary>
    public string? Message { get; set; }

    /// <summary>Recomendacion correctiva si el veredicto no es Pass.</summary>
    public string? Recommendation { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Alias de TimestampUtc para claridad semantica en reglas.</summary>
    public DateTime EvaluatedAt
    {
        get => TimestampUtc;
        set => TimestampUtc = value;
    }

    public string EngineVersion { get; set; } = "0.1.0";
}

/// <summary>Resultado posible de una comprobacion normativa.</summary>
public enum CheckVerdictCode
{
    Unknown,
    Pass,
    Warning,
    Fail,
    Error,
    NotApplicable
}

/// <summary>
/// Referencia normativa localizada (codigo, version, articulo, parrafo, titulo).
/// </summary>
public class NormReference
{
    public string Code { get; set; } = string.Empty;           // CTE_DB_SE_C, CE_RD_470_2021
    public string Version { get; set; } = string.Empty;        // edicion
    public string Article { get; set; } = string.Empty;        // 4.3.1
    public string? Paragraph { get; set; }                     // parrafo o apartado
    public string? Title { get; set; }                         // texto humano en espanol
    public string? TitleEs { get => Title; set => Title = value; }   // alias
}

/// <summary>
/// Hipotesis explicita empleada por la regla (p.ej. "se desprecia rozamiento en trasdos").
/// </summary>
public class Hypothesis
{
    public string Name { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
}

/// <summary>
/// Variable simbolica del calculo con trazabilidad de su origen.
/// </summary>
public class CalcVariable
{
    public string Symbol { get; set; } = string.Empty;         // q_u, phi, N_c
    public string Name { get; set; } = string.Empty;           // Presion ultima, angulo de rozamiento...
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;           // kPa, deg, m

    /// <summary>Alias de Unit para conveniencia ("Units").</summary>
    public string Units { get => Unit; set => Unit = value; }

    /// <summary>Alias de Name, descripcion legible.</summary>
    public string Description { get => Name; set => Name = value; }

    public string? Source { get; set; }                        // Borehole-03 @ 5.50m, user, derived
}

/// <summary>
/// Paso intermedio del calculo, con su propia formula y variables.
/// </summary>
public class CalcStep
{
    public string Description { get; set; } = string.Empty;
    public string? FormulaLatex { get; set; }
    public List<CalcVariable> Variables { get; set; } = new();
    public CalcVariable? Result { get; set; }
}
