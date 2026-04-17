using CadZapatas.Reinforcement;
using ClosedXML.Excel;

namespace CadZapatas.Documentation;

/// <summary>
/// Exporta el despiece (Bar Bending Schedule) a una hoja de calculo XLSX compatible con
/// las convenciones espanolas: columnas marca, diametro, acero, forma, longitud, cantidad,
/// peso unitario, peso total.
/// </summary>
public static class BarScheduleXlsx
{
    public static void Export(BarBendingSchedule schedule, string outputPath)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Despiece");

        // Encabezado
        ws.Cell(1, 1).Value = "DESPIECE DE ARMADURAS";
        ws.Range(1, 1, 1, 8).Merge().Style.Font.SetBold().Font.FontSize = 14;
        ws.Cell(2, 1).Value = $"Proyecto: {schedule.ProjectName}";
        ws.Cell(2, 8).Value = $"Generado: {schedule.GeneratedAt:dd/MM/yyyy HH:mm}";

        var headers = new[] { "Marca", "Elemento", "Ø (mm)", "Acero", "Forma",
                              "Long. desarr. (m)", "Cantidad", "Peso total (kg)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 5;
        foreach (var r in schedule.Rows.OrderBy(x => x.DiameterMm).ThenBy(x => x.Mark))
        {
            ws.Cell(row, 1).Value = r.Mark;
            ws.Cell(row, 2).Value = r.ElementName;
            ws.Cell(row, 3).Value = r.DiameterMm;
            ws.Cell(row, 4).Value = r.SteelGrade;
            ws.Cell(row, 5).Value = r.ShapeCode;
            ws.Cell(row, 6).Value = Math.Round(r.DevelopedLengthM, 2);
            ws.Cell(row, 7).Value = r.Quantity;
            ws.Cell(row, 8).Value = Math.Round(r.TotalWeightKg, 2);
            row++;
        }

        // Totales
        ws.Cell(row + 1, 7).Value = "TOTAL:";
        ws.Cell(row + 1, 7).Style.Font.Bold = true;
        ws.Cell(row + 1, 8).Value = Math.Round(schedule.TotalSteelKg, 2);
        ws.Cell(row + 1, 8).Style.Font.Bold = true;

        // Subtotal por diametro
        row += 3;
        ws.Cell(row, 1).Value = "RESUMEN POR DIAMETRO";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "Ø (mm)";
        ws.Cell(row, 2).Value = "Peso (kg)";
        ws.Range(row, 1, row, 2).Style.Font.Bold = true;
        row++;
        foreach (var kv in schedule.WeightsByDiameterKg.OrderBy(k => k.Key))
        {
            ws.Cell(row, 1).Value = kv.Key;
            ws.Cell(row, 2).Value = Math.Round(kv.Value, 2);
            row++;
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(outputPath);
    }
}
