using CadZapatas.Core.Bim;
using CadZapatas.Core.Primitives;

namespace CadZapatas.Foundations;

/// <summary>
/// Base comun de toda cimentacion.
/// </summary>
public abstract class Foundation : StructuralElement
{
    /// <summary>Profundidad de cimentacion (elevacion de la cara inferior).</summary>
    public double FoundingElevation { get; set; }

    /// <summary>Espesor de hormigon de limpieza (m). Por defecto 10 cm.</summary>
    public double LeanConcreteThickness { get; set; } = 0.10;

    /// <summary>Hormigon de limpieza: clase (HL-150, HM-15, HM-20).</summary>
    public string LeanConcreteClass { get; set; } = "HL-150/B/20";

    /// <summary>True si la zapata esta en contacto con el nivel freatico.</summary>
    public bool InContactWithWaterTable { get; set; }

    /// <summary>Cargas de diseno traducidas al centro de la cara inferior (en ejes globales).</summary>
    public DesignActions DesignActions { get; set; } = new();
}

/// <summary>
/// Resultante de acciones caracteristicas y/o de calculo sobre la cara inferior.
/// </summary>
public class DesignActions
{
    public double N_kN { get; set; }       // axil vertical (compresion positiva)
    public double Vx_kN { get; set; }      // cortante direccion X
    public double Vy_kN { get; set; }      // cortante direccion Y
    public double Mx_kNm { get; set; }     // momento eje X
    public double My_kNm { get; set; }     // momento eje Y
    public double Mz_kNm { get; set; }     // torsor (raramente relevante en cimentacion)
    public string? ReferenceCombination { get; set; }
}
