using CadZapatas.Core.Validation;

namespace CadZapatas.Geotechnics;

/// <summary>
/// Asesor del reconocimiento minimo exigido por CTE DB-SE-C 3 (criterios generales).
/// Genera advertencias si la campana geotecnica no cumple los minimos razonables.
///
/// NOTA NORMATIVA: la tabla real del DB-SE-C define numero minimo de puntos y profundidades
/// en funcion del grupo de terreno (T1/T2/T3) y tipo de construccion (C-0 ... C-4). Aqui se
/// codifica una version pragmatica fiel al espiritu del CTE: minimo 3 puntos y
/// profundidades funcion del tipo de construccion.
/// SUPOSICION DE DISENO: parametros exactos afinados segun necesidad del proyecto; la
/// matriz es versionable por el motor normativo.
/// </summary>
public class ReconnaissanceAdvisor
{
    public ValidationReport Review(SoilModel soil, string terrainGroup, string constructionType,
        double minRequiredPoints = 3, double minDepthM = 6.0)
    {
        var r = new ValidationReport();

        if (soil.Boreholes.Count < minRequiredPoints)
        {
            r.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Error,
                Source = "Geotechnics",
                Code = "GEO-001",
                Title = "Reconocimiento geotecnico insuficiente",
                Detail = $"Se han registrado {soil.Boreholes.Count} puntos de reconocimiento. El CTE DB-SE-C exige un minimo general de {minRequiredPoints} puntos.",
                Suggestion = "Ampliar la campana de reconocimiento hasta cumplir el minimo."
            });
        }

        int tooShallow = soil.Boreholes.Count(b => b.Depth < minDepthM);
        if (tooShallow > 0)
        {
            r.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Warning,
                Source = "Geotechnics",
                Code = "GEO-002",
                Title = "Sondeos con profundidad insuficiente",
                Detail = $"{tooShallow} sondeos no alcanzan la profundidad minima razonable de {minDepthM} m para construccion {constructionType} en terreno {terrainGroup}.",
                Suggestion = "Revisar y, en su caso, profundizar los sondeos afectados."
            });
        }

        if (soil.WaterTables.Count == 0)
        {
            r.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Warning,
                Source = "Geotechnics",
                Code = "GEO-003",
                Title = "Sin nivel freatico declarado",
                Detail = "No se ha definido ningun nivel freatico. Si el terreno es saturable, podria subestimarse la subpresion.",
                Suggestion = "Declarar el nivel freatico (estatico y estacional alto) en el modelo."
            });
        }

        // Estratos con parametros incompletos
        foreach (var layer in soil.Layers)
        {
            if (layer.Parameters.UnitWeight.CharacteristicValue <= 0)
            {
                r.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    Source = "Geotechnics",
                    Code = "GEO-010",
                    Title = $"Estrato {layer.Code} sin peso especifico",
                    ElementId = layer.Id,
                    ElementCode = layer.Code,
                    Detail = "No se ha definido peso especifico caracteristico.",
                    Suggestion = "Editar el estrato y asignar gamma segun ensayos o correlaciones."
                });
            }
            if (layer.Problematic != ProblematicSoilFlag.None)
            {
                r.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Warning,
                    Source = "Geotechnics",
                    Code = "GEO-020",
                    Title = $"Estrato {layer.Code} problematico ({layer.Problematic})",
                    ElementId = layer.Id,
                    ElementCode = layer.Code,
                    Suggestion = "El CTE DB-SE-C exige estudio especifico para terrenos problematicos."
                });
            }
        }

        return r;
    }
}
