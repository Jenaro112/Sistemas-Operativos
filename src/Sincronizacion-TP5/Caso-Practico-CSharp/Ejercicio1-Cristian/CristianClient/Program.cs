using CristianClient;

// ============================================================
//  Ejercicio 1 - Cliente que sincroniza con el algoritmo de Cristian
// ============================================================

var cfg = ClientConfig.FromEnvironment();
ExecLog.Init(cfg.Name);

// Reloj "del sistema operativo" del servidor A/B/C (simulado, con su desfase inicial).
// Este reloj NUNCA se modifica.
var localClock = new SimulatedClock(TimeSpan.FromSeconds(cfg.InitialOffsetSeconds), cfg.DriftPpm);

// Reloj lógico de la aplicación: es el que se ajusta.
var appClock = new ApplicationClock(localClock);

using var http = new HttpClient
{
    BaseAddress = new Uri(cfg.TimeServerUrl.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromMilliseconds(cfg.RequestTimeoutMs)
};

ExecLog.Write($"Cliente '{cfg.Name}' iniciado. Hora local inicial: {Fmt.Time(localClock.Now)} " +
              $"(desfase {cfg.InitialOffsetSeconds:+0.###;-0.###;0} s, deriva {cfg.DriftPpm} ppm)");
ExecLog.Write($"Servidor de tiempo: {cfg.TimeServerUrl} | muestras por ronda: {cfg.Samples} | " +
              $"RTT máx: {cfg.MaxRttMs} ms | timeout: {cfg.RequestTimeoutMs} ms | intervalo: {cfg.SyncIntervalSeconds} s");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// Espera inicial para que el servidor de tiempo termine de arrancar.
await Task.Delay(TimeSpan.FromSeconds(cfg.StartupDelaySeconds));

var sync = new CristianSynchronizer(cfg, localClock, appClock, http);
var round = 0;
while (!cts.IsCancellationRequested && (cfg.MaxRounds == 0 || round < cfg.MaxRounds))
{
    round++;
    await sync.RunRoundAsync(cts.Token);
    if (cfg.MaxRounds != 0 && round >= cfg.MaxRounds) break;
    try { await Task.Delay(TimeSpan.FromSeconds(cfg.SyncIntervalSeconds), cts.Token); }
    catch (TaskCanceledException) { }
}

ExecLog.Write($"Cliente finalizado. Hora SO: {Fmt.Time(localClock.Now)} | Hora app: {Fmt.Time(appClock.Now)}");

public sealed record ClientConfig(
    string Name,
    string TimeServerUrl,
    double InitialOffsetSeconds,
    double DriftPpm,
    int Samples,
    int MaxRttMs,
    int RequestTimeoutMs,
    double SyncIntervalSeconds,
    double StartupDelaySeconds,
    int MaxRounds)
{
    public static ClientConfig FromEnvironment() => new(
        Name: Env.Str("CLIENT_NAME", "servidor-A"),
        TimeServerUrl: Env.Str("TIME_SERVER_URL", "http://localhost:5100"),
        InitialOffsetSeconds: Env.Num("INITIAL_OFFSET_SECONDS", 0),
        DriftPpm: Env.Num("CLOCK_DRIFT_PPM", 0),
        Samples: Env.Int("SAMPLES", 3),
        MaxRttMs: Env.Int("MAX_RTT_MS", 1000),
        RequestTimeoutMs: Env.Int("REQUEST_TIMEOUT_MS", 3000),
        SyncIntervalSeconds: Env.Num("SYNC_INTERVAL_SECONDS", 10),
        StartupDelaySeconds: Env.Num("STARTUP_DELAY_SECONDS", 2),
        MaxRounds: Env.Int("MAX_ROUNDS", 0));
}
