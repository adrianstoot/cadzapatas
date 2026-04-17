namespace CadZapatas.Reinforcement;

/// <summary>
/// Despiece (bar bending schedule). Tabla de despiece agregada del proyecto, con filas
/// agrupadas por marca, diametro y acero. Formato compatible con UNE-EN ISO 3766 y
/// convenciones espanolas de listas de doblado.
/// </summary>
public class BarBendingSchedule
{
    public string ProjectName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public List<ScheduleRow> Rows { get; set; } = new();

    public double TotalSteelKg => Rows.Sum(r => r.TotalWeightKg);

    public Dictionary<int, double> WeightsByDiameterKg
        => Rows.GroupBy(r => r.DiameterMm)
               .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalWeightKg));

    public Dictionary<string, double> WeightsBySteelGradeKg
        => Rows.GroupBy(r => r.SteelGrade)
               .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalWeightKg));

    public void AddBar(RebarBar bar, string elementName)
    {
        Rows.Add(new ScheduleRow
        {
            Mark = bar.Mark,
            ElementName = elementName,
            DiameterMm = bar.DiameterMm,
            SteelGrade = bar.SteelGrade,
            ShapeCode = bar.ShapeCode,
            DevelopedLengthM = bar.DevelopedLengthM,
            Quantity = bar.Quantity,
            UnitWeightKgPerMeter = bar.UnitWeightKgPerMeter
        });
    }

    public void AddStirrup(Stirrup s, string elementName)
    {
        Rows.Add(new ScheduleRow
        {
            Mark = s.Mark,
            ElementName = elementName,
            DiameterMm = s.DiameterMm,
            SteelGrade = s.SteelGrade,
            ShapeCode = $"Cerco-{s.Shape}",
            DevelopedLengthM = s.DevelopedLengthPerStirrupM,
            Quantity = s.Count,
            UnitWeightKgPerMeter = 0.00617 * s.DiameterMm * s.DiameterMm
        });
    }
}

/// <summary>
/// Fila del despiece correspondiente a un grupo de barras identicas.
/// </summary>
public class ScheduleRow
{
    public string Mark { get; set; } = string.Empty;
    public string ElementName { get; set; } = string.Empty;
    public int DiameterMm { get; set; }
    public string SteelGrade { get; set; } = "B500SD";
    public string ShapeCode { get; set; } = "00";
    public double DevelopedLengthM { get; set; }
    public int Quantity { get; set; }
    public double UnitWeightKgPerMeter { get; set; }

    public double TotalLengthM => DevelopedLengthM * Quantity;
    public double TotalWeightKg => TotalLengthM * UnitWeightKgPerMeter;
}
