using CadZapatas.Core.Audit;
using CadZapatas.Core.Bim;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CadZapatas.Documentation;

/// <summary>
/// Genera la memoria de calculo en PDF usando QuestPDF. Incluye portada, indice,
/// datos de partida, comprobaciones (tabla con verdicts) y anejo con trazas completas.
/// </summary>
public static class CalculationReportPdf
{
    /// <summary>Inicializa QuestPDF con licencia Community (gratis para open source).</summary>
    public static void ConfigureLicense() => QuestPDF.Settings.License = LicenseType.Community;

    public static void Generate(string outputPath, Project project, IEnumerable<CalcTrace> traces)
    {
        ConfigureLicense();
        var list = traces.ToList();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                page.Header().Element(h => ComposeHeader(h, project));
                page.Content().Element(c => ComposeContent(c, project, list));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Memoria de calculo generada por CadZapatas  -  Pagina ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf(outputPath);
    }

    private static void ComposeHeader(IContainer container, Project project)
    {
        container.PaddingBottom(5).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("MEMORIA DE CALCULO").FontSize(18).Bold();
                col.Item().Text($"Proyecto: {project.Name}").FontSize(11);
                col.Item().Text($"Ingeniero: {project.EngineerName ?? "-"}   -   {DateTime.Now:dd/MM/yyyy}").FontSize(9);
            });
            row.ConstantItem(120).AlignRight().Text("CadZapatas 0.1.0").FontSize(9).Italic();
        });
    }

    private static void ComposeContent(IContainer container, Project project, List<CalcTrace> traces)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Spacing(12);

            col.Item().Text("1. DATOS GENERALES").FontSize(13).Bold();
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                AddRow(t, "Proyecto", project.Name);
                AddRow(t, "Referencia", project.ReferenceNumber ?? "-");
                AddRow(t, "Cliente", project.ClientName ?? "-");
                AddRow(t, "Localidad", project.CityProvince ?? "-");
                AddRow(t, "Ingeniero", project.EngineerName ?? "-");
                AddRow(t, "Normativa geotecnica", project.GeotechnicalNormCode);
                AddRow(t, "Normativa estructural", project.StructuralNormCode);
                AddRow(t, "Grupo de terreno (CTE)", project.TerrainGroup);
                AddRow(t, "Tipo de construccion (CTE)", project.ConstructionType);
            });

            col.Item().PaddingTop(10).Text("2. RESUMEN DE COMPROBACIONES").FontSize(13).Bold();
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);     // elemento
                    c.RelativeColumn(3);     // chequeo
                    c.RelativeColumn(2);     // norma
                    c.RelativeColumn(1);     // util
                    c.RelativeColumn(1);     // verd
                });
                t.Header(h =>
                {
                    h.Cell().Text("Elemento").Bold();
                    h.Cell().Text("Comprobacion").Bold();
                    h.Cell().Text("Norma").Bold();
                    h.Cell().Text("η").Bold();
                    h.Cell().Text("Resultado").Bold();
                });
                foreach (var tr in traces)
                {
                    t.Cell().Text(tr.ElementType);
                    t.Cell().Text(tr.CheckName);
                    t.Cell().Text($"{tr.Norm.Code} {tr.Norm.Article}");
                    t.Cell().Text($"{tr.Utilization:F2}");
                    t.Cell().Text(tr.Verdict.ToString())
                        .FontColor(tr.Verdict == CheckVerdictCode.Pass ? Colors.Green.Darken2
                                 : tr.Verdict == CheckVerdictCode.Warning ? Colors.Orange.Darken1
                                 : Colors.Red.Darken2);
                }
            });

            col.Item().PaddingTop(12).Text("3. TRAZAS DETALLADAS").FontSize(13).Bold();
            foreach (var tr in traces)
            {
                col.Item().Element(e => RenderTrace(e, tr));
            }
        });
    }

    private static void AddRow(TableDescriptor t, string label, string value)
    {
        t.Cell().Padding(2).Text(label).Italic();
        t.Cell().Padding(2).Text(value ?? "");
    }

    private static void RenderTrace(IContainer container, CalcTrace trace)
    {
        container.PaddingVertical(6).Border(1).BorderColor(Colors.Grey.Lighten2)
          .Padding(8).Column(col =>
        {
            col.Item().Text(tx =>
            {
                tx.Span($"{trace.CheckId}  -  ").Bold();
                tx.Span(trace.CheckName);
            });
            col.Item().Text($"{trace.Norm.Code} Art. {trace.Norm.Article}  -  {trace.Norm.Title}")
                .FontSize(9).Italic();

            if (!string.IsNullOrWhiteSpace(trace.FormulaLatex))
                col.Item().PaddingTop(4).Text($"  {trace.FormulaLatex}").FontFamily("Courier").FontSize(9);

            if (trace.Inputs.Count > 0)
            {
                col.Item().PaddingTop(4).Text("Datos de entrada:").Italic();
                foreach (var v in trace.Inputs)
                    col.Item().Text($"   {v.Symbol} = {v.Value:G4} {v.Unit}").FontSize(9);
            }

            col.Item().PaddingTop(4).Row(r =>
            {
                r.RelativeItem().Text($"Resultado: {trace.Result.Symbol} = {trace.Result.Value:F3} {trace.Result.Unit}");
                r.RelativeItem().Text($"Limite: {trace.Limit.Symbol} = {trace.Limit.Value:F3} {trace.Limit.Unit}");
                r.RelativeItem().Text($"η = {trace.Utilization:F3}");
            });
            col.Item().PaddingTop(2).Text(trace.Verdict.ToString())
                .Bold()
                .FontColor(trace.Verdict == CheckVerdictCode.Pass ? Colors.Green.Darken2
                         : trace.Verdict == CheckVerdictCode.Warning ? Colors.Orange.Darken1
                         : Colors.Red.Darken2);
            if (!string.IsNullOrWhiteSpace(trace.Message))
                col.Item().Text(trace.Message).FontSize(9);
        });
    }
}
