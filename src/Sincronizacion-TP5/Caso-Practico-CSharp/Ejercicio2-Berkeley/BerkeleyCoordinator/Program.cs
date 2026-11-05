using BerkeleyCoordinator;

// ============================================================
//  Ejercicio 2 - Coordinador del algoritmo de Berkeley
// ============================================================

var cfg = CoordinatorConfig.FromEnvironment();
ExecLog.Init(cfg.Name);

var clock = new SimulatedClock(TimeSpan.FromSeconds(cfg.InitialOffsetSeconds), cfg.DriftPpm);
using var http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(cfg.RequestTimeoutMs) };

ExecLog.Write($"Coordinador '{cfg.Name}' iniciado. Hora: {Fmt.Time(clock.Now)} " +
              $"(desfase {cfg.InitialOffsetSeconds:+0.###;-0.###;0} s, deriva {cfg.DriftPpm} ppm)");
ExecLog.Write($"Nodos: {string.Join(", ", cfg.Nodes.Select(n => $"{n.Name}={n.Url}"))}");
ExecLog.Write($"Intervalo: {cfg.SyncIntervalSeconds} s | timeout: {cfg.RequestTimeoutMs} ms | " +
              $"desvío máximo para el promedio: {(cfg.MaxDeviationSeconds > 0 ? cfg.MaxDeviationSeconds + " s" : "sin límite (promedio simple)")}");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

await Task.Delay(TimeSpan.FromSeconds(cfg.StartupDelaySeconds));

var berkeley = new BerkeleyAlgorithm(cfg, clock, http);
var round = 0;
while (!cts.IsCancellationRequested && (cfg.MaxRounds == 0 || round < cfg.MaxRounds))
{
    round++;
    try { await berkeley.RunRoundAsync(cts.Token); }
    catch (Exception ex) when (ex is not OperationCanceledException) { ExecLog.Write($"Error en la ronda {round}: {ex.Message}"); }
    if (cfg.MaxRounds != 0 && round >= cfg.MaxRounds) break;
    // ---- 6. Repetir después de un intervalo configurable ----
    try { await Task.Delay(TimeSpan.FromSeconds(cfg.SyncIntervalSeconds), cts.Token); }
    catch (TaskCanceledException) { }
}

ExecLog.Write("Coordinador finalizado.");

public sealed record CoordinatorConfig(
    string Name,
    double InitialOffsetSeconds,
    double DriftPpm,
    List<(string Name, string Url)> Nodes,
    double SyncIntervalSeconds,
    double MaxDeviationSeconds,
    int RequestTimeoutMs,
    double StartupDelaySeconds,
    int MaxRounds)
{
    /// <summary>NODES="nodo-1=http://nodo-1:8080;nodo-2=http://nodo-2:8080;..."</summary>
    public static CoordinatorConfig FromEnvironment() => new(
        Name: Env.Str("COORDINATOR_NAME", "coordinador"),
        InitialOffsetSeconds: Env.Num("INITIAL_OFFSET_SECONDS", 0),
        DriftPpm: Env.Num("CLOCK_DRIFT_PPM", 0),
        Nodes: Env.Str("NODES", "nodo-1=http://localhost:5201;nodo-2=http://localhost:5202;nodo-3=http://localhost:5203")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split('=', 2))
            .Select(x => (x[0], x[1].TrimEnd('/')))
            .ToList(),
        SyncIntervalSeconds: Env.Num("SYNC_INTERVAL_SECONDS", 15),
        MaxDeviationSeconds: Env.Num("MAX_DEVIATION_SECONDS", 0),
        RequestTimeoutMs: Env.Int("REQUEST_TIMEOUT_MS", 2000),
        StartupDelaySeconds: Env.Num("STARTUP_DELAY_SECONDS", 3),
        MaxRounds: Env.Int("MAX_ROUNDS", 0));
}
