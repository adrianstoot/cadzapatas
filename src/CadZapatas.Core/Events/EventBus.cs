namespace CadZapatas.Core.Events;

/// <summary>
/// Bus de eventos intraproceso sencillo, sin dependencias.
/// Permite desacoplar modulos: un editor publica GeometryChanged y el calculo se invalida.
/// </summary>
public interface IEventBus
{
    void Publish<T>(T @event) where T : IAppEvent;
    IDisposable Subscribe<T>(Action<T> handler) where T : IAppEvent;
}

public interface IAppEvent
{
    DateTime TimestampUtc { get; }
}

public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _lock = new();

    public void Publish<T>(T @event) where T : IAppEvent
    {
        List<Delegate>? handlers;
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(typeof(T), out handlers)) return;
            handlers = handlers.ToList(); // snapshot para no bloquear durante dispatch
        }
        foreach (var h in handlers)
        {
            try { ((Action<T>)h)(@event); }
            catch { /* no propagar errores de suscriptor */ }
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : IAppEvent
    {
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _subscribers[typeof(T)] = list;
            }
            list.Add(handler);
        }
        return new Subscription(() =>
        {
            lock (_lock)
            {
                if (_subscribers.TryGetValue(typeof(T), out var list))
                    list.Remove(handler);
            }
        });
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _dispose;
        public Subscription(Action dispose) { _dispose = dispose; }
        public void Dispose() { _dispose?.Invoke(); _dispose = null; }
    }
}

public record GeometryChangedEvent(Guid ElementId, string ElementType) : IAppEvent
{
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;
}

public record CalculationInvalidatedEvent(Guid ElementId, string Reason) : IAppEvent
{
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;
}

public record CalculationCompletedEvent(Guid ElementId, int ChecksPassed, int ChecksFailed) : IAppEvent
{
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;
}

public record ProjectOpenedEvent(Guid ProjectId, string FilePath) : IAppEvent
{
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;
}
