# CadZapatas

**Software CAD/BIM de cimentaciones para España — normativa-first**

CadZapatas es una aplicación de escritorio 100% local para el diseño, cálculo y documentación de cimentaciones (zapatas aisladas, corridas, losas, pilotes y muros de contención) bajo la normativa técnica española vigente.

> ⚠️ Sin IA, sin APIs, sin servicios online. Todo cálculo se realiza en la máquina del usuario y con trazabilidad completa a norma.

## Marco normativo

- **CTE DB-SE-C** (Documento Básico Seguridad Estructural — Cimientos). Capacidad portante, asientos, grupos de terreno T1/T2/T3, tipos de construcción C-0 a C-4.
- **Código Estructural (Real Decreto 470/2021)**. Hormigón y armaduras: fck/fcm/fctm (Art. 40.4), Ecm, cuantías geométricas/mecánicas, anclajes, solapes, recubrimientos (Art. 37), despiece (Art. 55).
- **EHE-08** solo como compatibilidad legacy (proyectos previos al RD 470/2021).
- **FIEBDC-3/2020** para mediciones/presupuestos (BC3).

## Funcionalidades

- Modelado BIM de cimentaciones: `IsolatedFooting`, `MatFoundation`, `Pile`, `RetainingWall`.
- Modelo geotécnico por estratos con niveles freáticos, parámetros característicos y de diseño.
- **Cálculos geotécnicos:** Brinch-Hansen (hundimiento), deslizamiento, vuelco, asientos (elástico, edométrico, Burland-Burbidge), empujes activo/pasivo/reposo (Rankine/Coulomb).
- **Cálculos estructurales:** flexión, cortante sin cepos (CE 44.2.3.2.1), punzonamiento, cuantías mínimas, separación de barras, fisuración.
- **Armado:** barras, mallazos, cercos con códigos de forma, ganchos y patillas; tabla de despiece.
- **Motor normativo trazable:** cada comprobación genera un `CalcTrace` con norma, fórmula LaTeX, entradas, resultado, límite, utilización y veredicto.
- **Documentación:** memoria de cálculo en **PDF** (QuestPDF), despiece en **XLSX** (ClosedXML), planos en **DXF** (AutoCAD R12 ASCII), mediciones en **BC3** (FIEBDC-3).
- **Persistencia:** formato propio `.czap` (SQLite + JSON blobs) con log de operaciones y trazas.
- **Visor 3D** integrado (HelixToolkit) en WPF.

## Stack técnico

- **.NET 8** / C# 12 (nullable)
- **WPF** + **CommunityToolkit.Mvvm** (MVVM)
- **HelixToolkit.Wpf** (visor 3D)
- **EF Core + SQLite** (persistencia)
- **Clipper2Lib** (booleanas 2D), **NetTopologySuite** (GIS/geometría), **MathNet.Numerics** (numérico)
- **QuestPDF** (PDF), **ClosedXML** (XLSX)
- **xUnit** (tests)

## Estructura de la solución

```
src/
  CadZapatas.Core             Modelo BIM, TrackedParameter, CalcTrace, unidades SI
  CadZapatas.Geometry         Primitivas y operaciones geométricas
  CadZapatas.Geotechnics      SoilModel, estratos, niveles freáticos
  CadZapatas.Foundations      IsolatedFooting, MatFoundation, Pile
  CadZapatas.Retaining        Muros de contención
  CadZapatas.Reinforcement    Barras, mallazos, cercos, despiece
  CadZapatas.Materials        Hormigones y aceros CE/EHE-08
  CadZapatas.Calculation      BearingCapacity, Stability, Settlement, secciones
  CadZapatas.NormativeEngine  Reglas normativas con trazabilidad
  CadZapatas.Persistence      .czap (SQLite + JSON), ProjectRepository
  CadZapatas.Documentation    Memoria PDF, despiece XLSX, DXF
  CadZapatas.Quantities       Mediciones + BC3 FIEBDC-3
  CadZapatas.Desktop          Aplicación WPF
tests/
  CadZapatas.Core.Tests
  CadZapatas.Calculation.Tests
  CadZapatas.Geotechnics.Tests
samples/
  SampleProject               Demo end-to-end: proyecto → cálculo → PDF + BC3
```

## Arranque rápido

Prerrequisitos: Windows 10/11, **.NET 8 SDK**.

```powershell
dotnet restore
dotnet build -c Release
dotnet test
dotnet run --project samples/SampleProject
```

Para la aplicación gráfica:

```powershell
dotnet run --project src/CadZapatas.Desktop
```

Ver [BUILD.md](BUILD.md) para detalles.

## Unidades internas

Todo el núcleo trabaja en **SI**: metros, Pascales, Newtons, kilogramos, grados/radianes. Las conversiones a kN, MPa, cm, etc. se realizan en la capa de presentación y documentación.

## Licencia

Uso interno. QuestPDF se utiliza bajo **Community License** (empresas con ingresos < 1M USD/año).

## Estado

Versión 0.1.0 — núcleo funcional, pendiente pulido de UI y ampliación del catálogo de reglas normativas.
