using CadZapatas.Calculation;
using CadZapatas.Core.Audit;
using CadZapatas.Core.Bim;
using CadZapatas.Documentation;
using CadZapatas.Foundations;
using CadZapatas.Geotechnics;
using CadZapatas.Materials;
using CadZapatas.Persistence;
using CadZapatas.Quantities;
using CadZapatas.Retaining;

namespace CadZapatas.Samples;

/// <summary>
/// Proyecto de ejemplo que demuestra el flujo completo:
///   1. Crear proyecto + modelo de suelo
///   2. Definir zapata aislada y muro
///   3. Ejecutar comprobaciones CTE DB-SE-C + Codigo Estructural
///   4. Generar memoria de calculo en PDF
///   5. Generar mediciones BC3
///   6. Guardar archivo .czap (SQLite)
/// Ejecutar: dotnet run --project samples/SampleProject
/// </summary>
internal class Program
{
    static int Main()
    {
        // 1. Proyecto
        var project = new Project
        {
            Name = "Ejemplo CadZapatas - Edificio residencial",
            Code = "P-DEMO-001",
            ClientName = "Cliente Demostracion S.L.",
            EngineerName = "Adrian (Ingeniero)",
            CityProvince = "Madrid",
            ReferenceNumber = "EJ-2026-001",
            TerrainGroup = "T2",
            ConstructionType = "C-2"
        };

        // 2. Modelo de suelo
        var soil = new SoilModel { Name = "Arcilla firme sobre grava densa" };
        soil.Layers.Add(new SoilLayer
        {
            Name = "Arcilla firme",
            Kind = SoilKind.Clay,
            TopElevation = 0, BottomElevation = -4,
            Parameters = new SoilParameterSet
            {
                UnitWeight = TrackedParameter.Fixed(19000, "N/m3"),
                FrictionAngleDegrees = TrackedParameter.Fixed(25, "deg"),
                CohesionEffective = TrackedParameter.Fixed(20000, "Pa"),
                DeformationModulus = TrackedParameter.Fixed(15e6, "Pa")
            }
        });

        // 3. Zapata y muro
        var zapata = new IsolatedFooting
        {
            Code = "ZAP-P1",
            Name = "Zapata pilar P1",
            Length = 2.40, Width = 2.40, Thickness = 0.60,
            ColumnLengthX = 0.40, ColumnLengthY = 0.40,
            ConcreteClass = "HA-25",
            RebarSteelGrade = "B500SD",
            ExposureClass = "XC2",
            NominalCover = 0.05,
            DesignActions = new DesignActions
            {
                N_kN = 1100, Vx_kN = 30, Mx_kNm = 50
            }
        };

        var muro = new RetainingWall
        {
            Code = "MUR-01",
            Name = "Muro trasdos 4 m",
            Height = 4.00,
            StemThicknessTop = 0.30,
            StemThicknessBottom = 0.40,
            ToeLength = 0.70,
            HeelLength = 1.60,
            FoundationThickness = 0.50,
            ConcreteClass = "HA-30",
            SurchargeKPa = 10
        };

        // 4. Comprobaciones
        var concrete = ConcreteMaterial.ByDesignation("HA-25");
        var steel = RebarSteelMaterial.ByGrade("B500SD");

        var traces = new List<CalcTrace>();
        var footCalc = new IsolatedFootingCalculator();
        traces.AddRange(footCalc.Run(zapata, soil.Layers[0].Parameters, concrete, steel));

        var wallCalc = new RetainingWallCalculator();
        traces.AddRange(wallCalc.Run(muro, soil.Layers[0].Parameters, soil.Layers[0].Parameters));

        Console.WriteLine($"=== Proyecto: {project.Name}");
        Console.WriteLine($"=== {traces.Count} comprobaciones ejecutadas");
        Console.WriteLine();
        foreach (var t in traces)
        {
            string mark = t.Verdict switch
            {
                CheckVerdictCode.Pass => "[OK]",
                CheckVerdictCode.Fail => "[KO]",
                CheckVerdictCode.Warning => "[!!]",
                _ => "[??]"
            };
            Console.WriteLine($"{mark} {t.ElementType,-16} {t.CheckName,-38} " +
                              $"η={t.Utilization:F2}   {t.Norm.Code} {t.Norm.Article}");
        }
        Console.WriteLine();

        // 5. Memoria PDF
        var outDir = Path.Combine(AppContext.BaseDirectory, "output");
        Directory.CreateDirectory(outDir);
        var pdfPath = Path.Combine(outDir, "memoria_calculo.pdf");
        CalculationReportPdf.Generate(pdfPath, project, traces);
        Console.WriteLine($"PDF generado: {pdfPath}");

        // 6. Mediciones BC3
        var to = new QuantityTakeoff();
        var budget = to.GenerateBudget(project.Name,
                                        new Foundation[] { zapata },
                                        new[] { muro },
                                        Array.Empty<CadZapatas.Reinforcement.ReinforcementLayout>());
        var bc3Path = Path.Combine(outDir, "presupuesto.bc3");
        Bc3Writer.Export(budget, bc3Path);
        Console.WriteLine($"BC3 generado: {bc3Path}  ({budget.Items.Count} partidas, {budget.TotalAmount:F0} EUR)");

        // 7. Archivo .czap
        var czapPath = Path.Combine(outDir, "proyecto.czap");
        var file = new ProjectFile { Project = project };
        file.ElementsByType["IsolatedFooting"] = new List<object> { zapata };
        file.ElementsByType["RetainingWall"] = new List<object> { muro };
        new ProjectRepository(czapPath).CreateNew(file);
        Console.WriteLine($"CZAP guardado: {czapPath}");

        return 0;
    }
}
