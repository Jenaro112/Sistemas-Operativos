namespace CristianClient;

/// <summary>
/// Reloj LÓGICO DE APLICACIÓN.
/// No modifica el reloj del sistema operativo (aquí representado por <see cref="SimulatedClock"/>):
/// guarda una corrección que se suma a la hora local cada vez que se la consulta.
///
///     horaApp = horaLocal + corrección
/// </summary>
public sealed class ApplicationClock(SimulatedClock localClock)
{
    private readonly object _lock = new();
    private TimeSpan _correction = TimeSpan.Zero;

    /// <summary>Corrección actual respecto del reloj local.</summary>
    public TimeSpan Correction
    {
        get { lock (_lock) return _correction; }
    }

    public DateTime Now => localClock.Now + Correction;

    /// <summary>Hora de la aplicación correspondiente a un instante del reloj local.</summary>
    public DateTime At(DateTime localTime) => localTime + Correction;

    public void SetCorrection(TimeSpan correction)
    {
        lock (_lock) _correction = correction;
    }
}
