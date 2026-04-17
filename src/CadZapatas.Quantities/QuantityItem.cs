namespace CadZapatas.Quantities;

/// <summary>
/// Partida de medicion. Aglutina un codigo BC3, descripcion, unidad y cantidad.
/// </summary>
public class QuantityItem
{
    public string Code { get; set; } = string.Empty;         // codigo BC3 (ej. 03HAB10010)
    public string Description { get; set; } = string.Empty;  // descripcion larga
    public string Unit { get; set; } = "m3";                 // m3, m2, m, kg, ud
    public double Quantity { get; set; }
    public double UnitPrice { get; set; }                    // EUR/unidad
    public double Amount => Math.Round(Quantity * UnitPrice, 2);

    public string Category { get; set; } = string.Empty;     // Capitulo (Movimiento de tierras, Cimentaciones...)
    public string? ElementId { get; set; }                   // enlace al objeto BIM que origina la medicion
    public string? Notes { get; set; }
}

/// <summary>Presupuesto agrupado por capitulos.</summary>
public class Budget
{
    public string ProjectName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<QuantityItem> Items { get; set; } = new();

    public double TotalAmount => Items.Sum(i => i.Amount);

    public Dictionary<string, double> AmountsByCategory
        => Items.GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
}
