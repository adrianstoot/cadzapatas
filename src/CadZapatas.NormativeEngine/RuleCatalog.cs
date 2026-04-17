namespace CadZapatas.NormativeEngine;

/// <summary>
/// Catalogo de reglas normativas en memoria. El catalogo se construye al iniciar la aplicacion
/// registrando reglas (hardcoded o cargadas desde JSON/XML). El motor usa el catalogo para
/// encontrar, validar y ejecutar reglas aplicables a un elemento.
/// </summary>
public class RuleCatalog
{
    private readonly Dictionary<string, NormRule> _rules = new();

    /// <summary>Codigo de la norma principal activa (ej. "CTE_DB_SE_C_2019").</summary>
    public string ActiveGeotechnicalCode { get; set; } = "CTE_DB_SE_C_2019";

    /// <summary>Norma estructural activa (ej. "CE_RD_470_2021").</summary>
    public string ActiveStructuralCode { get; set; } = "CE_RD_470_2021";

    public void Register(NormRule rule)
    {
        if (_rules.ContainsKey(rule.RuleId))
            throw new InvalidOperationException($"Regla '{rule.RuleId}' ya registrada.");
        _rules[rule.RuleId] = rule;
    }

    public NormRule? Get(string ruleId) => _rules.TryGetValue(ruleId, out var r) ? r : null;

    public IEnumerable<NormRule> All() => _rules.Values;

    public IEnumerable<NormRule> GetByObjectType(string objectType)
        => _rules.Values.Where(r => r.AppliesToObjectTypes.Contains(objectType));

    public IEnumerable<NormRule> GetByKind(CheckKind kind)
        => _rules.Values.Where(r => r.Kind == kind);

    public int Count => _rules.Count;

    /// <summary>
    /// Carga el catalogo estandar del proyecto (reglas del CTE DB-SE-C y del Codigo Estructural).
    /// Las reglas concretas las registra cada modulo de calculo llamando a Register().
    /// </summary>
    public static RuleCatalog CreateStandard()
    {
        var catalog = new RuleCatalog();
        StandardRuleRegistrations.RegisterAll(catalog);
        return catalog;
    }
}
