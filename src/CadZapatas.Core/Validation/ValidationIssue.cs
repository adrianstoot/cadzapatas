namespace CadZapatas.Core.Validation;

/// <summary>
/// Incidencia detectada durante validacion (geometrica, normativa, constructiva).
/// </summary>
public class ValidationIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IssueSeverity Severity { get; set; } = IssueSeverity.Info;
    public string Source { get; set; } = string.Empty;     // Modulo originador: Geotechnics, Calculation, Rebar...
    public string Code { get; set; } = string.Empty;       // GEO-001, RBR-014, FND-022
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public Guid? ElementId { get; set; }
    public string? ElementCode { get; set; }
    public string? Suggestion { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

public enum IssueSeverity
{
    Info,           // Informativo
    Warning,        // Advertencia: revisar
    Error,          // Error: no cumple
    Critical        // Critico: imposibilidad fisica, colision grave, incumplimiento grave
}

public interface IValidationReport
{
    IReadOnlyList<ValidationIssue> Issues { get; }
    int CountBy(IssueSeverity sev);
    bool HasErrors { get; }
}

public class ValidationReport : IValidationReport
{
    private readonly List<ValidationIssue> _issues = new();
    public IReadOnlyList<ValidationIssue> Issues => _issues;
    public void Add(ValidationIssue issue) => _issues.Add(issue);
    public void AddRange(IEnumerable<ValidationIssue> issues) => _issues.AddRange(issues);
    public int CountBy(IssueSeverity sev) => _issues.Count(i => i.Severity == sev);
    public bool HasErrors => _issues.Any(i => i.Severity >= IssueSeverity.Error);
}
