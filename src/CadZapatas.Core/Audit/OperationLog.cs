namespace CadZapatas.Core.Audit;

/// <summary>
/// Entrada del log de operaciones. Cada accion significativa del usuario o del motor
/// genera una entrada inmutable. Permite auditoria y base para undo/redo persistente.
/// </summary>
public class OperationLogEntry
{
    public long SeqId { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string OperationType { get; set; } = string.Empty;  // CreateObject, EditProperty, Calculate...
    public Guid? ObjectId { get; set; }
    public string? ObjectType { get; set; }
    public string? User { get; set; }
    public string ForwardDeltaJson { get; set; } = "{}";       // diff aplicable
    public string ReverseDeltaJson { get; set; } = "{}";       // diff inverso para undo
    public string? Note { get; set; }
}

public interface IOperationLogWriter
{
    Task AppendAsync(OperationLogEntry entry, CancellationToken cancellation = default);
}

public interface IOperationLogReader
{
    Task<IReadOnlyList<OperationLogEntry>> GetRecentAsync(int count, CancellationToken cancellation = default);
    Task<IReadOnlyList<OperationLogEntry>> GetByObjectAsync(Guid objectId, CancellationToken cancellation = default);
}
