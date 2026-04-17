using System.Text.Json;
using System.Text.Json.Serialization;
using CadZapatas.Core.Audit;

namespace CadZapatas.Persistence;

/// <summary>
/// Repositorio de alto nivel para leer/guardar archivos .czap. Usa JSON indented
/// para legibilidad humana y compatibilidad diff (git-friendly).
/// </summary>
public class ProjectRepository
{
    public string FilePath { get; }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public ProjectRepository(string filePath) => FilePath = filePath;

    /// <summary>Crea un nuevo archivo .czap vacio con su esquema.</summary>
    public void CreateNew(ProjectFile file)
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        using var db = new CzapDbContext(FilePath);
        db.Database.EnsureCreated();
        var payload = JsonSerializer.Serialize(file, JsonOptions);
        db.ProjectBlobs.Add(new ProjectBlobEntity { JsonPayload = payload });
        db.Metadata.Add(new FileMetadataEntity { Key = "schema.version", Value = "1" });
        db.Metadata.Add(new FileMetadataEntity { Key = "application", Value = "CadZapatas 0.1.0" });
        db.SaveChanges();
    }

    /// <summary>Carga el proyecto completo.</summary>
    public ProjectFile Load()
    {
        if (!File.Exists(FilePath))
            throw new FileNotFoundException($"Archivo de proyecto no encontrado: {FilePath}");
        using var db = new CzapDbContext(FilePath);
        var blob = db.ProjectBlobs.OrderByDescending(b => b.SavedAt).FirstOrDefault()
            ?? throw new InvalidDataException("El archivo no contiene proyecto.");
        return JsonSerializer.Deserialize<ProjectFile>(blob.JsonPayload, JsonOptions)
            ?? throw new InvalidDataException("JSON de proyecto invalido.");
    }

    /// <summary>Guarda el proyecto reemplazando el blob (una sola version activa).</summary>
    public void Save(ProjectFile file)
    {
        file.ModifiedAt = DateTime.UtcNow;
        using var db = new CzapDbContext(FilePath);
        db.Database.EnsureCreated();
        var payload = JsonSerializer.Serialize(file, JsonOptions);
        var existing = db.ProjectBlobs.OrderByDescending(b => b.SavedAt).FirstOrDefault();
        if (existing != null)
        {
            existing.JsonPayload = payload;
            existing.SavedAt = DateTime.UtcNow;
        }
        else
        {
            db.ProjectBlobs.Add(new ProjectBlobEntity { JsonPayload = payload });
        }
        db.SaveChanges();
    }

    /// <summary>Anade una traza de calculo al log.</summary>
    public void AppendTrace(CalcTrace trace)
    {
        using var db = new CzapDbContext(FilePath);
        db.Database.EnsureCreated();
        db.CalcTraces.Add(new CalcTraceEntity
        {
            ElementId = trace.ElementId,
            ElementType = trace.ElementType,
            CheckId = trace.CheckId,
            CheckName = trace.CheckName,
            NormCode = trace.Norm.Code,
            NormArticle = trace.Norm.Article,
            Utilization = trace.Utilization,
            Verdict = trace.Verdict.ToString(),
            Message = trace.Message,
            JsonDetail = JsonSerializer.Serialize(trace, JsonOptions),
            Timestamp = trace.TimestampUtc
        });
        db.SaveChanges();
    }

    /// <summary>Anade una operacion al log para futuro undo/redo.</summary>
    public void AppendOperation(string type, string? description, string? jsonDelta = null)
    {
        using var db = new CzapDbContext(FilePath);
        db.Database.EnsureCreated();
        db.OperationLog.Add(new OperationLogEntity
        {
            OperationType = type,
            Description = description,
            JsonDelta = jsonDelta,
            UserName = Environment.UserName
        });
        db.SaveChanges();
    }

    /// <summary>Lee las trazas mas recientes del archivo.</summary>
    public List<CalcTraceEntity> RecentTraces(int limit = 200)
    {
        using var db = new CzapDbContext(FilePath);
        db.Database.EnsureCreated();
        return db.CalcTraces.OrderByDescending(t => t.Timestamp).Take(limit).ToList();
    }
}
