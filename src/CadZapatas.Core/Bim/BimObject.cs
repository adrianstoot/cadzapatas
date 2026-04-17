using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CadZapatas.Core.Bim;

/// <summary>
/// Clase base de todo objeto BIM. Carga identidad, geometria, propiedades, estado de calculo,
/// estado normativo y trazabilidad. Todo objeto persistible en el modelo hereda de aqui.
/// </summary>
public abstract class BimObject : INotifyPropertyChanged
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;        // Codigo legible: Z-001, P-001, M-001
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    private BimObjectState _state = BimObjectState.Draft;
    public BimObjectState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; set; }

    public Guid? ProjectId { get; set; }
    public Guid? LevelId { get; set; }
    public Guid? PhaseId { get; set; }

    /// <summary>
    /// Propiedades dinamicas (clave-valor) para datos no modelados en el esquema fuerte.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Tipo discriminador para persistencia y dispatch.
    /// </summary>
    public abstract string ObjectType { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        ModifiedUtc = DateTime.UtcNow;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString() => $"{ObjectType} {Code} ({Name})";
}

public enum BimObjectState
{
    Draft,          // Sin calcular
    Calculated,     // Con resultado de calculo
    Verified,       // Cumple normativa
    Failed,         // No cumple normativa
    Detailed,       // Con armaduras completas
    Documented,     // Con plano generado
    Locked          // Bloqueado para edicion
}
