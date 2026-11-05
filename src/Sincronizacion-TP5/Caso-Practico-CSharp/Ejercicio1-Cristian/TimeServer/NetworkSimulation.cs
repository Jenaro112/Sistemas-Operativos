namespace TimeServer;

/// <summary>
/// Parámetros para simular la red y fallas (se pueden cambiar en caliente desde /api/admin).
/// - RequestDelayMs : demora del "viaje de ida" (antes de leer el reloj).
/// - ResponseDelayMs: demora del "viaje de vuelta" (después de leer el reloj).
///   Si ida != vuelta la red es ASIMÉTRICA y Cristian comete error (supone RTT/2 en cada sentido).
/// - JitterMs       : variación aleatoria extra en cada sentido.
/// - Down           : el servidor deja de responder (las solicitudes quedan colgadas).
/// </summary>
public sealed class NetworkSimulation
{
    public int RequestDelayMs { get; set; }
    public int ResponseDelayMs { get; set; }
    public int JitterMs { get; set; }
    public bool Down { get; set; }

    public TimeSpan NextRequestDelay() => TimeSpan.FromMilliseconds(RequestDelayMs + Jitter());
    public TimeSpan NextResponseDelay() => TimeSpan.FromMilliseconds(ResponseDelayMs + Jitter());

    private int Jitter() => JitterMs <= 0 ? 0 : Random.Shared.Next(0, JitterMs + 1);
}
