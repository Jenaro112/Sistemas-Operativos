namespace BerkeleyNode;

/// <summary>
/// Vigila que el coordinador siga sincronizando. Si pasa más de COORDINATOR_TIMEOUT_SECONDS
/// sin recibir un ajuste, avisa en el log: el nodo sigue funcionando con su propio reloj,
/// pero los relojes del grupo empiezan a separarse por la deriva.
/// (Punto de partida para tolerar la falla del coordinador: aquí se podría iniciar una elección.)
/// </summary>
public sealed class CoordinatorWatchdog(NodeState state, SimulatedClock clock) : BackgroundService
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(Env.Num("COORDINATOR_TIMEOUT_SECONDS", 45));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var warned = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            var silence = DateTime.UtcNow - state.LastAdjustmentUtc;
            if (silence > _timeout && !warned)
            {
                warned = true;
                ExecLog.Write($"!! Sin ajustes del coordinador hace {silence.TotalSeconds:0} s. " +
                              $"Se continúa con el reloj local ({Fmt.Time(clock.Now)}, deriva {clock.DriftPpm} ppm). " +
                              "El grupo deja de estar sincronizado.");
            }
            else if (silence <= _timeout && warned)
            {
                warned = false;
                ExecLog.Write("Coordinador activo nuevamente: se reanudan los ajustes.");
            }
        }
    }
}
