using System.Globalization;
using System.Text;

namespace CadZapatas.Quantities;

/// <summary>
/// Escritor del formato BC3 (Base de Datos de la Construccion) en version FIEBDC-3/2020.
/// Formato utilizado en Espana para intercambiar mediciones y presupuestos con CYPE, PRESTO y ARQUIMEDES.
/// Implementa los registros ~C (concepto), ~D (descomposicion), ~T (texto), ~M (medicion), ~V (version).
/// Referencia: www.fiebdc.es
/// </summary>
public static class Bc3Writer
{
    public static void Export(Budget budget, string outputPath)
    {
        var sb = new StringBuilder();
        var ci = CultureInfo.InvariantCulture;

        // Version
        sb.Append("~V|BC3 FIEBDC-3/2020|CadZapatas 0.1.0|")
          .Append(DateTime.UtcNow.ToString("ddMMyyyy", ci))
          .AppendLine("|EUR|");

        // Raiz
        sb.Append("~C|RAIZ||").Append(budget.ProjectName).AppendLine("|EUR||");

        // Concepto raiz con sus capitulos
        var capitulos = budget.Items.GroupBy(i => i.Category).ToList();

        foreach (var cap in capitulos)
        {
            string capCode = $"CAP{cap.Key.Replace(" ", "_").ToUpper()}";
            sb.Append("~C|").Append(capCode).Append("||").Append(cap.Key).AppendLine("|EUR||");
            // Descomposicion del raiz en el capitulo
            sb.Append("~D|RAIZ|").Append(capCode).Append("\\").Append(1.ToString(ci))
              .AppendLine("\\|");

            foreach (var item in cap)
            {
                // Concepto de la partida
                sb.Append("~C|").Append(item.Code).Append("|").Append(item.Unit).Append("|")
                  .Append(item.Description).Append("|")
                  .Append(item.UnitPrice.ToString("0.00", ci)).AppendLine("||");

                // Descomposicion capitulo -> partida
                sb.Append("~D|").Append(capCode).Append("|").Append(item.Code).Append("\\")
                  .Append(item.Quantity.ToString("0.000", ci)).AppendLine("\\|");

                // Medicion detallada (un unico subtotal)
                sb.Append("~M|").Append(capCode).Append(item.Code).Append("|\\\\")
                  .Append(item.Quantity.ToString("0.000", ci)).Append("\\")
                  .AppendLine("|");

                // Texto (descripcion extendida)
                if (!string.IsNullOrWhiteSpace(item.Notes))
                    sb.Append("~T|").Append(item.Code).Append("|").Append(item.Notes).AppendLine("|");
            }
        }

        // BC3 clasico usa ISO-8859-1; para maxima compatibilidad sin depender de CodePages
        // serializamos en UTF-8 (aceptado por versiones recientes de PRESTO y ARQUIMEDES).
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }
}
