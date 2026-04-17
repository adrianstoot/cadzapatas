using System.Globalization;
using System.Text;
using CadZapatas.Core.Primitives;

namespace CadZapatas.Documentation;

/// <summary>
/// Escritor DXF minimal (ASCII, AutoCAD R12+). Soporta LINE, CIRCLE, TEXT y POLYLINE.
/// Suficiente para exportar planos 2D de replanteo y armado. No depende de librerias externas.
/// </summary>
public class DxfWriter
{
    private readonly StringBuilder _sb = new();

    public DxfWriter()
    {
        // Cabecera DXF minima
        _sb.AppendLine("0");
        _sb.AppendLine("SECTION");
        _sb.AppendLine("2");
        _sb.AppendLine("ENTITIES");
    }

    public void AddLine(double x1, double y1, double x2, double y2, string layer = "0")
    {
        _sb.AppendLine("0");
        _sb.AppendLine("LINE");
        _sb.AppendLine("8"); _sb.AppendLine(layer);
        _sb.AppendLine("10"); _sb.AppendLine(Fmt(x1));
        _sb.AppendLine("20"); _sb.AppendLine(Fmt(y1));
        _sb.AppendLine("11"); _sb.AppendLine(Fmt(x2));
        _sb.AppendLine("21"); _sb.AppendLine(Fmt(y2));
    }

    public void AddCircle(double cx, double cy, double radius, string layer = "0")
    {
        _sb.AppendLine("0");
        _sb.AppendLine("CIRCLE");
        _sb.AppendLine("8"); _sb.AppendLine(layer);
        _sb.AppendLine("10"); _sb.AppendLine(Fmt(cx));
        _sb.AppendLine("20"); _sb.AppendLine(Fmt(cy));
        _sb.AppendLine("40"); _sb.AppendLine(Fmt(radius));
    }

    public void AddText(double x, double y, double height, string text, string layer = "0")
    {
        _sb.AppendLine("0");
        _sb.AppendLine("TEXT");
        _sb.AppendLine("8"); _sb.AppendLine(layer);
        _sb.AppendLine("10"); _sb.AppendLine(Fmt(x));
        _sb.AppendLine("20"); _sb.AppendLine(Fmt(y));
        _sb.AppendLine("40"); _sb.AppendLine(Fmt(height));
        _sb.AppendLine("1"); _sb.AppendLine(text);
    }

    public void AddRectangle(double x, double y, double w, double h, string layer = "0")
    {
        AddLine(x, y, x + w, y, layer);
        AddLine(x + w, y, x + w, y + h, layer);
        AddLine(x + w, y + h, x, y + h, layer);
        AddLine(x, y + h, x, y, layer);
    }

    public void AddPolyline(IEnumerable<Point2D> points, bool closed, string layer = "0")
    {
        var list = points.ToList();
        _sb.AppendLine("0");
        _sb.AppendLine("POLYLINE");
        _sb.AppendLine("8"); _sb.AppendLine(layer);
        _sb.AppendLine("66"); _sb.AppendLine("1");
        _sb.AppendLine("10"); _sb.AppendLine("0");
        _sb.AppendLine("20"); _sb.AppendLine("0");
        _sb.AppendLine("70"); _sb.AppendLine(closed ? "1" : "0");
        foreach (var p in list)
        {
            _sb.AppendLine("0");
            _sb.AppendLine("VERTEX");
            _sb.AppendLine("8"); _sb.AppendLine(layer);
            _sb.AppendLine("10"); _sb.AppendLine(Fmt(p.X));
            _sb.AppendLine("20"); _sb.AppendLine(Fmt(p.Y));
        }
        _sb.AppendLine("0");
        _sb.AppendLine("SEQEND");
    }

    public void Save(string filePath)
    {
        _sb.AppendLine("0"); _sb.AppendLine("ENDSEC");
        _sb.AppendLine("0"); _sb.AppendLine("EOF");
        File.WriteAllText(filePath, _sb.ToString(), Encoding.ASCII);
    }

    private static string Fmt(double v) => v.ToString("0.######", CultureInfo.InvariantCulture);
}
