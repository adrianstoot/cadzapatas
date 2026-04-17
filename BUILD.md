# Compilación de CadZapatas

## Prerrequisitos

- **Windows 10/11** (requerido por WPF en `CadZapatas.Desktop`).
- **.NET 8 SDK** — https://dotnet.microsoft.com/download/dotnet/8.0
- Git (opcional, para clonar el repositorio).

Para compilar solo los módulos de núcleo y ejecutar tests, basta con el SDK de .NET 8 en cualquier plataforma (Windows, Linux, macOS) — **solo el proyecto Desktop requiere Windows**.

## Clonar

```powershell
git clone https://github.com/<usuario>/cadzapatas.git
cd cadzapatas
```

## Restaurar dependencias

```powershell
dotnet restore
```

## Compilar

```powershell
dotnet build -c Release
```

## Ejecutar tests

```powershell
dotnet test
```

Los tests cubren:

- `CadZapatas.Core.Tests` — Modelo BIM, `Point3D`, `CalcTrace`, `CheckVerdictCode`.
- `CadZapatas.Calculation.Tests` — Factores de Brinch-Hansen (Nq, Nc, Nγ), presión última.
- `CadZapatas.Geotechnics.Tests` — `SoilModel`, estratos, `TrackedParameter`, peso sumergido.

## Ejecutar el ejemplo end-to-end

```powershell
dotnet run --project samples/SampleProject
```

El ejemplo genera en la carpeta de salida:

- `sample.czap` — proyecto persistido (SQLite).
- `memoria.pdf` — memoria de cálculo.
- `mediciones.bc3` — mediciones FIEBDC-3.

## Ejecutar la aplicación WPF

```powershell
dotnet run --project src/CadZapatas.Desktop
```

La barra de herramientas ofrece: **Nuevo**, **Abrir .czap**, **Guardar como**, **+ Zapata**, **+ Muro**, **Comprobar todo**, **Memoria PDF**, **BC3**.

## Publicación (binario autocontenido Windows x64)

```powershell
dotnet publish src/CadZapatas.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El binario resultante queda en `src/CadZapatas.Desktop/bin/Release/net8.0-windows/win-x64/publish/`.

## Troubleshooting

- **`NU1202` — paquete no compatible**: verifica que todos los proyectos targetean `net8.0` (salvo `CadZapatas.Desktop` que usa `net8.0-windows`).
- **`CS8600` / nullable warnings**: el proyecto usa `Nullable` activado; ajusta `<WarningsAsErrors>` en `Directory.Build.props` si molesta durante desarrollo.
- **SQLite nativo**: `Microsoft.Data.Sqlite` incluye el binario nativo; no requiere instalar SQLite aparte.
- **QuestPDF**: se declara `Community License` en `App.xaml.cs`. No tocar salvo cambio de licencia.
