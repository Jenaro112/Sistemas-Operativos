namespace Lamport.Common;

/// <summary>
/// Reloj lógico de Lamport. Reglas:
///   - Evento interno:      L = L + 1
///   - Envío de mensaje:    L = L + 1  y el mensaje lleva L
///   - Recepción de mensaje con marca T:  L = max(L, T) + 1
/// Es thread-safe porque un servicio web atiende varias solicitudes en paralelo.
/// </summary>
public sealed class LamportClock
{
    private readonly object _lock = new();
    private long _value;

    public long Value
    {
        get { lock (_lock) return _value; }
    }

    /// <summary>Evento interno o envío: incrementa en 1. Devuelve (antes, después).</summary>
    public (long Before, long After) Tick()
    {
        lock (_lock)
        {
            var before = _value;
            _value++;
            return (before, _value);
        }
    }

    /// <summary>Recepción: L = max(L, remoto) + 1. Devuelve (antes, después).</summary>
    public (long Before, long After) Receive(long remote)
    {
        lock (_lock)
        {
            var before = _value;
            _value = Math.Max(_value, remote) + 1;
            return (before, _value);
        }
    }
}
