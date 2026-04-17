using System.Collections.ObjectModel;
using System.IO;
using CadZapatas.Calculation;
using CadZapatas.Core.Audit;
using CadZapatas.Core.Bim;
using CadZapatas.Documentation;
using CadZapatas.Foundations;
using CadZapatas.Geotechnics;
using CadZapatas.Materials;
using CadZapatas.Persistence;
using CadZapatas.Quantities;
using CadZapatas.Reinforcement;
using CadZapatas.Retaining;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace CadZapatas.Desktop.ViewModels;

/// <summary>
/// ViewModel raiz de la ventana principal. Gestiona el proyecto activo, la coleccion
/// de elementos estructurales, el modelo de suelo, las trazas de comprobacion normativa
/// y los comandos de archivo/calculo/exportacion.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private Project _project = CreateSampleProject();
    [ObservableProperty] private SoilModel _soilModel = CreateSampleSoil();
    [ObservableProperty] private string _statusText = "Proyecto nuevo iniciado.";
    [ObservableProperty] private string? _currentFilePath;

    public ObservableCollection<IsolatedFooting> Footings { get; } = new();
    public ObservableCollection<RetainingWall> Walls { get; } = new();
    public ObservableCollection<Pile> Piles { get; } = new();
    public ObservableCollection<CalcTrace> Traces { get; } = new();
    public ObservableCollection<ReinforcementLayout> Reinforcement { get; } = new();

    public MainViewModel()
    {
        // Poblar con un ejemplo para que el usuario vea algo al abrir.
        LoadSampleData();
    }

    [RelayCommand]
    private void NewProject()
    {
        Project = CreateSampleProject();
        SoilModel = CreateSampleSoil();
        Footings.Clear(); Walls.Clear(); Piles.Clear();
        Traces.Clear(); Reinforcement.Clear();
        CurrentFilePath = null;
        StatusText = "Proyecto nuevo.";
    }

    [RelayCommand]
    private void AddFooting()
    {
        var f = new IsolatedFooting
        {
            Code = $"ZAP-{Footings.Count + 1:D3}",
            Name = $"Zapata {Footings.Count + 1}",
            Length = 2.00, Width = 2.00, Thickness = 0.60,
            ColumnLengthX = 0.40, ColumnLengthY = 0.40,
            ConcreteClass = "HA-25",
            DesignActions = new() { N_kN = 850, Vx_kN = 30, Mx_kNm = 45 }
        };
        Footings.Add(f);
        StatusText = $"Zapata {f.Code} anadida.";
    }

    [RelayCommand]
    private void AddWall()
    {
        var w = new RetainingWall
        {
            Code = $"MUR-{Walls.Count + 1:D3}",
            Name = $"Muro {Walls.Count + 1}",
            Height = 4.00,
            StemThicknessTop = 0.30,
            StemThicknessBottom = 0.40,
            ToeLength = 0.70,
            HeelLength = 1.40,
            FoundationThickness = 0.50,
            ConcreteClass = "HA-30",
            SurchargeKPa = 10
        };
        Walls.Add(w);
        StatusText = $"Muro {w.Code} anadido.";
    }

    [RelayCommand]
    private void RunAllChecks()
    {
        Traces.Clear();
        var concrete = ConcreteMaterial.ByDesignation("HA-25");
        var steel = RebarSteelMaterial.ByGrade("B500SD");
        var backfill = CreateSampleSoil().Layers.First().Parameters;
        var foundationSoil = CreateSampleSoil().Layers.Last().Parameters;

        var footingCalc = new IsolatedFootingCalculator();
        foreach (var f in Footings)
        {
            var layer = SoilModel.LayerAtElevation(f.FoundingElevation) ??
                        SoilModel.Layers.FirstOrDefault();
            if (layer == null) continue;
            foreach (var t in footingCalc.Run(f, layer.Parameters, concrete, steel))
                Traces.Add(t);
        }

        var wallCalc = new RetainingWallCalculator();
        foreach (var w in Walls)
        {
            foreach (var t in wallCalc.Run(w, backfill, foundationSoil))
                Traces.Add(t);
        }

        var pileCalc = new PileCalculator();
        foreach (var p in Piles)
            Traces.Add(pileCalc.Run(p, SoilModel));

        StatusText = $"Comprobaciones: {Traces.Count} (OK {Traces.Count(t => t.Verdict == CheckVerdictCode.Pass)}, KO {Traces.Count(t => t.Verdict == CheckVerdictCode.Fail)}).";
    }

    [RelayCommand]
    private void SaveAs()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Proyecto CadZapatas (*.czap)|*.czap",
            DefaultExt = "czap",
            FileName = Project.Name.Replace(' ', '_') + ".czap"
        };
        if (dlg.ShowDialog() != true) return;
        var file = BuildProjectFile();
        new ProjectRepository(dlg.FileName).CreateNew(file);
        CurrentFilePath = dlg.FileName;
        StatusText = $"Guardado: {dlg.FileName}";
    }

    [RelayCommand]
    private void OpenProject()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Proyecto CadZapatas (*.czap)|*.czap"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var file = new ProjectRepository(dlg.FileName).Load();
            Project = file.Project;
            CurrentFilePath = dlg.FileName;
            StatusText = $"Abierto: {dlg.FileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error abriendo: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportPdf()
    {
        if (Traces.Count == 0) { StatusText = "Ejecute primero las comprobaciones."; return; }
        var dlg = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", DefaultExt = "pdf",
                                         FileName = Project.Name + "_memoria.pdf" };
        if (dlg.ShowDialog() != true) return;
        CalculationReportPdf.Generate(dlg.FileName, Project, Traces);
        StatusText = $"PDF generado: {dlg.FileName}";
    }

    [RelayCommand]
    private void ExportBc3()
    {
        var to = new QuantityTakeoff();
        var budget = to.GenerateBudget(Project.Name, Footings.Cast<Foundation>()
                                                             .Concat(Piles.Cast<Foundation>()),
                                        Walls, Reinforcement);
        var dlg = new SaveFileDialog { Filter = "BC3 (*.bc3)|*.bc3", DefaultExt = "bc3" };
        if (dlg.ShowDialog() != true) return;
        Bc3Writer.Export(budget, dlg.FileName);
        StatusText = $"BC3 generado: {dlg.FileName} ({budget.Items.Count} partidas, {budget.TotalAmount:F0} EUR).";
    }

    private ProjectFile BuildProjectFile()
    {
        var pf = new ProjectFile { Project = Project };
        pf.ElementsByType["IsolatedFooting"] = Footings.Cast<object>().ToList();
        pf.ElementsByType["RetainingWall"] = Walls.Cast<object>().ToList();
        pf.ElementsByType["Pile"] = Piles.Cast<object>().ToList();
        return pf;
    }

    private static Project CreateSampleProject() => new()
    {
        Name = "Proyecto CadZapatas de ejemplo",
        Code = "P-001",
        EngineerName = "Ingeniero",
        ClientName = "Cliente Modelo S.L.",
        CityProvince = "Madrid",
        ReferenceNumber = "REF-2026-001",
        GeotechnicalNormCode = "CTE_DB_SE_C_2019",
        StructuralNormCode = "CE_RD_470_2021",
        TerrainGroup = "T2",
        ConstructionType = "C-2"
    };

    private static SoilModel CreateSampleSoil()
    {
        var m = new SoilModel { Name = "Suelo de ejemplo" };
        m.Layers.Add(new SoilLayer
        {
            Name = "Arcilla limosa firme",
            Kind = SoilKind.Clay,
            TopElevation = 0,
            BottomElevation = -4,
            Parameters = new SoilParameterSet
            {
                UnitWeight = TrackedParameter.Fixed(19000, "N/m3"),
                FrictionAngleDegrees = TrackedParameter.Fixed(25, "deg"),
                CohesionEffective = TrackedParameter.Fixed(20000, "Pa"),
                DeformationModulus = TrackedParameter.Fixed(15e6, "Pa")
            }
        });
        m.Layers.Add(new SoilLayer
        {
            Name = "Grava arenosa densa",
            Kind = SoilKind.SandyGravel,
            TopElevation = -4,
            BottomElevation = -15,
            Parameters = new SoilParameterSet
            {
                UnitWeight = TrackedParameter.Fixed(20000, "N/m3"),
                FrictionAngleDegrees = TrackedParameter.Fixed(35, "deg"),
                CohesionEffective = TrackedParameter.Fixed(0, "Pa"),
                SptNCharacteristic = TrackedParameter.Fixed(40, "golpes")
            }
        });
        return m;
    }

    private void LoadSampleData()
    {
        AddFooting();
        AddFooting();
        AddWall();
        RunAllChecksCommand.Execute(null);
    }
}
