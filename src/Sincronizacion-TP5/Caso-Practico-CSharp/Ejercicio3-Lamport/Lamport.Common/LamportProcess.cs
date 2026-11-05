using System.Text;

namespace Lamport.Common;

public enum EventKind { Interno, Envio, Recepcion }

/// <summary>Un evento registrado por un proceso.</summary>
public record EventRecord(
    string Process,
    EventKind Kind,
    string Description,
    DateTime PhysicalTime,
    long Lamport,
    string? Peer = null,
    long? MessageLamport = null,
    string? OrderId = null);

/// <summary>Mensaje entre servicios: siempre transporta la marca de Lamport del emisor.</summary>
public record LamportMessage<T>(string Sender, long Lamport, T Payload);

/// <summary>
/// Un proceso del sistema distribuido: combina su reloj físico (con desfase simulado),
/// su reloj de Lamport y el registro de eventos.
/// </summary>
public sealed class LamportProcess
{
    private readonly LamportClock _clock = new();
    private readonly List<EventRecord> _events = new();
    private readonly object _lock = new();
    private readonly TimeSpan _physicalOffset;

    public LamportProcess(string name, double physicalOffsetSeconds)
    {
        Name = name;
        _physicalOffset = TimeSpan.FromSeconds(physicalOffsetSeconds);
        ExecLog.Init(name);
        ExecLog.Write($"{name} iniciado. Reloj físico con desfase {physicalOffsetSeconds:+0.###;-0.###;0} s -> {Fmt.Time(PhysicalNow)} | Lamport = 0");
        ExecLog.Write(Header());
    }

    public string Name { get; }
    public long Lamport => _clock.Value;

    /// <summary>Reloj físico local (hora del SO + desfase simulado: cada servicio tiene su propio reloj).</summary>
    public DateTime PhysicalNow => DateTime.UtcNow + _physicalOffset;

    public IReadOnlyList<EventRecord> Events
    {
        get { lock (_lock) return _events.ToList(); }
    }

    /// <summary>Regla 1: evento interno.</summary>
    public EventRecord Internal(string description, string? orderId = null)
    {
        lock (_lock)
        {
            var (before, after) = _clock.Tick();
            return Record(new EventRecord(Name, EventKind.Interno, description, PhysicalNow, after, OrderId: orderId),
                $"{before}+1");
        }
    }

    /// <summary>Regla 2: envío de mensaje. Devuelve el mensaje con la marca de Lamport.</summary>
    public LamportMessage<T> Send<T>(string to, string description, T payload, string? orderId = null)
    {
        lock (_lock)
        {
            var (before, after) = _clock.Tick();
            Record(new EventRecord(Name, EventKind.Envio, description, PhysicalNow, after, to, after, orderId),
                $"{before}+1");
            return new LamportMessage<T>(Name, after, payload);
        }
    }

    /// <summary>Regla 3: recepción de mensaje. L = max(L, T) + 1.</summary>
    public EventRecord Receive<T>(LamportMessage<T> msg, string description, string? orderId = null)
    {
        lock (_lock)
        {
            var (before, after) = _clock.Receive(msg.Lamport);
            return Record(new EventRecord(Name, EventKind.Recepcion, description, PhysicalNow, after, msg.Sender, msg.Lamport, orderId),
                $"max({before},{msg.Lamport})+1");
        }
    }

    private EventRecord Record(EventRecord e, string rule)
    {
        _events.Add(e);
        ExecLog.Write(Row(e, rule));
        return e;
    }

    // ---------- Formato de tabla ----------

    public static string Header() =>
        $"| {"Proceso",-17}| {"Evento",-56}| {"Reloj físico",-13}| {"Lamport",7} | {"Regla / tipo",-21}|\n" +
        $"|{new string('-', 18)}|{new string('-', 57)}|{new string('-', 14)}|{new string('-', 9)}|{new string('-', 22)}|";

    public static string Row(EventRecord e, string? rule = null)
    {
        var desc = e.Description.Length > 55 ? e.Description[..55] : e.Description;
        return $"| {e.Process,-17}| {desc,-56}| {Fmt.Time(e.PhysicalTime),-13}| {e.Lamport,7} | {rule ?? KindText(e),-21}|";
    }

    private static string KindText(EventRecord e) => e.Kind switch
    {
        EventKind.Envio => $"envío -> {Short(e.Peer)}",
        EventKind.Recepcion => $"recep. <- {Short(e.Peer)}",
        _ => "interno"
    };

    private static string Short(string? s) => s?.Replace("Service", "") ?? "";

    /// <summary>Tabla de texto de un conjunto de eventos.</summary>
    public static string Table(IEnumerable<EventRecord> events)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header());
        foreach (var e in events) sb.AppendLine(Row(e));
        return sb.ToString();
    }
}
