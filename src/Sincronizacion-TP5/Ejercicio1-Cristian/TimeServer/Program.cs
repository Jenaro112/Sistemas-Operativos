using TimeServer;

// ============================================================
//  Ejercicio 1 - Servidor de tiempo (algoritmo de Cristian)
//  Expone su hora mediante GET /api/time
// ============================================================

var builder = WebApplication.CreateBuilder(args);

var name = Env.Str("SERVER_NAME", "servidor-tiempo");
var offsetSeconds = Env.Num("CLOCK_OFFSET_SECONDS", 2);   // en el enunciado: 10:00:02
var driftPpm = Env.Num("CLOCK_DRIFT_PPM", 0);

var network = new NetworkSimulation
{
    RequestDelayMs = Env.Int("REQUEST_DELAY_MS", 0),
    ResponseDelayMs = Env.Int("RESPONSE_DELAY_MS", 0),
    JitterMs = Env.Int("JITTER_MS", 0)
};

builder.Services.AddSingleton(new SimulatedClock(TimeSpan.FromSeconds(offsetSeconds), driftPpm));
builder.Services.AddSingleton(network);
builder.Services.AddControllers();

var app = builder.Build();

ExecLog.Init(name);
var clock = app.Services.GetRequiredService<SimulatedClock>();
ExecLog.Write($"Servidor de tiempo iniciado. Hora simulada: {Fmt.Time(clock.Now)} " +
              $"(desfase {offsetSeconds:+0.###;-0.###;0} s respecto del SO, deriva {driftPpm} ppm)");
ExecLog.Write($"Red simulada: ida={network.RequestDelayMs} ms, vuelta={network.ResponseDelayMs} ms, jitter={network.JitterMs} ms");

app.MapControllers();
app.Run();
