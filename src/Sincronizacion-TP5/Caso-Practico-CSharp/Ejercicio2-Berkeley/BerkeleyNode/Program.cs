using BerkeleyNode;

// ============================================================
//  Ejercicio 2 - Nodo participante del algoritmo de Berkeley
//  GET  /api/clock         -> devuelve su hora (reloj simulado)
//  POST /api/clock/adjust  -> aplica el ajuste que envía el coordinador
// ============================================================

var builder = WebApplication.CreateBuilder(args);

var name = Env.Str("NODE_NAME", "nodo-1");
var offsetSeconds = Env.Num("INITIAL_OFFSET_SECONDS", 0);
var driftPpm = Env.Num("CLOCK_DRIFT_PPM", 0);

builder.Services.AddSingleton(new NodeState { Name = name });
builder.Services.AddSingleton(new SimulatedClock(TimeSpan.FromSeconds(offsetSeconds), driftPpm));
builder.Services.AddHostedService<CoordinatorWatchdog>();
builder.Services.AddControllers();

var app = builder.Build();

ExecLog.Init(name);
var clock = app.Services.GetRequiredService<SimulatedClock>();
ExecLog.Write($"Nodo '{name}' iniciado. Hora simulada: {Fmt.Time(clock.Now)} " +
              $"(desfase {offsetSeconds:+0.###;-0.###;0} s, deriva {driftPpm} ppm)");

app.MapControllers();
app.Run();

namespace BerkeleyNode
{
    public sealed class NodeState
    {
        public required string Name { get; init; }
        /// <summary>Si es true el nodo simula estar desconectado (no responde).</summary>
        public bool Disconnected { get; set; }

        /// <summary>Último instante (reloj real) en que llegó un ajuste del coordinador.</summary>
        public DateTime LastAdjustmentUtc { get; set; } = DateTime.UtcNow;
    }
}
