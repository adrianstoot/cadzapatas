using Microsoft.EntityFrameworkCore;

namespace CadZapatas.Persistence;

/// <summary>
/// DbContext del archivo de proyecto .czap (SQLite). Almacena metadatos ligeros
/// en tablas relacionales y el documento completo serializado como JSON en una
/// unica fila ProjectBlob. Este enfoque evita la complejidad de mapear jerarquias
/// polimorficas BIM y facilita versionado del formato.
/// </summary>
public class CzapDbContext : DbContext
{
    public DbSet<ProjectBlobEntity> ProjectBlobs => Set<ProjectBlobEntity>();
    public DbSet<CalcTraceEntity> CalcTraces => Set<CalcTraceEntity>();
    public DbSet<OperationLogEntity> OperationLog => Set<OperationLogEntity>();
    public DbSet<FileMetadataEntity> Metadata => Set<FileMetadataEntity>();

    private readonly string _dbPath;

    public CzapDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectBlobEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.JsonPayload).IsRequired();
        });
        modelBuilder.Entity<CalcTraceEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ElementId);
            b.HasIndex(x => x.CheckId);
        });
        modelBuilder.Entity<OperationLogEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Timestamp);
        });
        modelBuilder.Entity<FileMetadataEntity>(b =>
        {
            b.HasKey(x => x.Key);
        });
    }
}

public class ProjectBlobEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string JsonPayload { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public int FormatVersion { get; set; } = 1;
}

public class CalcTraceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ElementId { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string CheckId { get; set; } = string.Empty;
    public string CheckName { get; set; } = string.Empty;
    public string NormCode { get; set; } = string.Empty;
    public string NormArticle { get; set; } = string.Empty;
    public double Utilization { get; set; }
    public string Verdict { get; set; } = "Unknown";
    public string? Message { get; set; }
    public string JsonDetail { get; set; } = string.Empty;    // CalcTrace completo serializado
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class OperationLogEntity
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string OperationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? UserName { get; set; }
    public string? JsonDelta { get; set; }        // cambio aplicado (undo/redo)
}

public class FileMetadataEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}
