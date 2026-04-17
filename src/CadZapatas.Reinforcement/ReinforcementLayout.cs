namespace CadZapatas.Reinforcement;

/// <summary>
/// Conjunto de armadura asociado a un elemento estructural. Agrupa barras, mallazos y estribos
/// y almacena los parametros globales del despiece (recubrimientos, aceros usados, clase de hormigon).
/// </summary>
public class ReinforcementLayout
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerElementId { get; set; }

    /// <summary>Clase de hormigon del elemento (condiciona lb y s_min).</summary>
    public string ConcreteClass { get; set; } = "HA-25";

    /// <summary>Clase de exposicion (condiciona recubrimiento).</summary>
    public string ExposureClass { get; set; } = "XC2";

    /// <summary>Recubrimiento nominal en mm (incluido margen de ejecucion, CE 37.2.4).</summary>
    public double NominalCoverMm { get; set; } = 35.0;

    /// <summary>Margen de ejecucion Δc_dur (normalmente 10 mm in situ, 5 mm prefab).</summary>
    public double ExecutionMarginMm { get; set; } = 10.0;

    /// <summary>Armaduras longitudinales / principales.</summary>
    public List<RebarBar> Bars { get; set; } = new();

    /// <summary>Mallas electrosoldadas (tipicas en losas, soleras, muros delgados).</summary>
    public List<RebarMesh> Meshes { get; set; } = new();

    /// <summary>Armaduras transversales (estribos, horquillas, helicoidales).</summary>
    public List<Stirrup> Stirrups { get; set; } = new();

    /// <summary>Armaduras de espera / arranques.</summary>
    public List<RebarBar> Starters { get; set; } = new();

    /// <summary>Peso total de acero de la pieza (kg).</summary>
    public double TotalSteelWeightKg
        => Bars.Sum(b => b.TotalWeightKg)
         + Meshes.Sum(m => m.TotalWeightKg)
         + Stirrups.Sum(s => s.TotalWeightKg)
         + Starters.Sum(b => b.TotalWeightKg);

    /// <summary>Cuantia geometrica global (kg de acero / m3 de hormigon).</summary>
    public double SteelRatioKgPerM3(double concreteVolumeM3)
        => concreteVolumeM3 > 1e-9 ? TotalSteelWeightKg / concreteVolumeM3 : 0.0;
}
