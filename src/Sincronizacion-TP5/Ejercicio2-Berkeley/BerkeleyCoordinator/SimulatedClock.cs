using System.Diagnostics;

namespace BerkeleyCoordinator;

/// <summary>
/// Reloj físico SIMULADO. No toca el reloj del sistema operativo:
/// parte de la hora real del SO y le suma un desfase (offset) y una deriva (drift).
///
///   Now = horaInicio + tiempoTranscurrido * (1 + driftPpm / 1e6) + offset
///
/// - offset  : diferencia inicial respecto de la hora real (ej. +8 s, -5 s).
/// - driftPpm: velocidad a la que el reloj se adelanta (+) o atrasa (-),
///             en partes por millón (1000 ppm = 1 ms por segundo).
/// </summary>
public sealed class SimulatedClock
{
    private readonly object _lock = new();
    private readonly DateTime _realStart = DateTime.UtcNow;
    private readonly Stopwatch _elapsed = Stopwatch.StartNew();
    private readonly double _driftFactor;
    private TimeSpan _offset;

    public SimulatedClock(TimeSpan initialOffset, double driftPpm = 0)
    {
        _offset = initialOffset;
        _driftFactor = 1 + driftPpm / 1_000_000d;
        DriftPpm = driftPpm;
    }

    public double DriftPpm { get; }

    /// <summary>Hora actual según este reloj (UTC).</summary>
    public DateTime Now
    {
        get
        {
            lock (_lock)
            {
                var elapsedTicks = (long)(_elapsed.Elapsed.Ticks * _driftFactor);
                return _realStart.AddTicks(elapsedTicks) + _offset;
            }
        }
    }

    /// <summary>Desfase acumulado respecto del reloj real del SO.</summary>
    public TimeSpan Offset
    {
        get { lock (_lock) return _offset; }
    }

    /// <summary>Adelanta (delta &gt; 0) o atrasa (delta &lt; 0) el reloj simulado.</summary>
    public void Adjust(TimeSpan delta)
    {
        lock (_lock) _offset += delta;
    }

    /// <summary>Fija un desfase absoluto (para inyectar fallas en las pruebas).</summary>
    public void SetOffset(TimeSpan offset)
    {
        lock (_lock) _offset = offset;
    }
}
