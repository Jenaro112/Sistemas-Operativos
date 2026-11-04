using System.Globalization;
using System.Net.Http.Json;
using System.Text;

namespace BerkeleyCoordinator;

public record ClockReading(string Node, DateTime Time);
public record AdjustRequest(int Round, double AdjustmentMs);
public record AdjustResponse(string Node, DateTime Before, DateTime After, double AdjustmentMs);

/// <summary>Lectura de un participante en una ronda.</summary>
public sealed class Participant
{
    public required string Name { get; init; }
    public string? Url { get; init; }                 // null = el propio coordinador
    public bool Responded { get; set; }
    public DateTime? ReadTime { get; set; }           // hora informada por el nodo
    public double RttMs { get; set; }
    public double DiffMs { get; set; }                // diferencia respecto del coordinador
    public bool InAverage { get; set; }
    public double AdjustmentMs { get; set; }
    public string? Error { get; set; }
    public bool IsCoordinator => Url is null;
}

/// <summary>
/// Algoritmo de Berkeley (coordinador):
///   1. Pide la hora a cada nodo (midiendo el RTT para compensar la demora: hora + RTT/2).
///   2. Calcula la diferencia de cada reloj respecto del suyo (el suyo = 0).
///   3. Promedia las diferencias (incluyendo la suya). Opcional: promedio tolerante a fallas,
///      descartando relojes que se alejan más de MAX_DEVIATION_SECONDS de la mediana.
///   4. Ajuste de cada participante = promedio - diferencia.
///   5. Envía a cada nodo SU ajuste (no la hora) y aplica el propio.
///   6. Se repite cada SYNC_INTERVAL_SECONDS.
/// </summary>
public sealed class BerkeleyAlgorithm(CoordinatorConfig cfg, SimulatedClock clock, HttpClient http)
{
    private int _round;

    public async Task RunRoundAsync(CancellationToken ct)
    {
        _round++;
        var sb = new StringBuilder();
        sb.AppendLine(new string('=', 96));
        sb.AppendLine($"RONDA {_round}  (coordinador: {cfg.Name})");

        // ---- 1. Solicitar la hora a cada nodo ----
        var participants = new List<Participant>
        {
            new() { Name = cfg.Name + " (coord)", Responded = true, ReadTime = clock.Now, DiffMs = 0 }
        };

        foreach (var (name, url) in cfg.Nodes)
        {
            var p = new Participant { Name = name, Url = url };
            participants.Add(p);
            var t0 = clock.Now;
            try
            {
                var r = await http.GetFromJsonAsync<ClockReading>($"{url}/api/clock", ct);
                var t1 = clock.Now;
                p.Responded = true;
                p.ReadTime = r!.Time;
                p.RttMs = (t1 - t0).TotalMilliseconds;
                // ---- 2. Diferencia: hora del nodo estimada en t1 (Cristian) menos la del coordinador ----
                var nodeAtT1 = r.Time.AddMilliseconds(p.RttMs / 2);
                p.DiffMs = (nodeAtT1 - t1).TotalMilliseconds;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                p.Responded = false;
                p.Error = ex is TaskCanceledException ? "timeout" : "sin conexión";
            }
        }

        var alive = participants.Where(p => p.Responded).ToList();

        // ---- 3. Promedio (opcionalmente tolerante a fallas) ----
        double median = Median(alive.Select(p => p.DiffMs));
        foreach (var p in alive)
            p.InAverage = cfg.MaxDeviationSeconds <= 0 || Math.Abs(p.DiffMs - median) <= cfg.MaxDeviationSeconds * 1000;
        var included = alive.Where(p => p.InAverage).ToList();
        var avg = included.Average(p => p.DiffMs);

        // ---- 4. Ajuste para cada participante ----
        foreach (var p in alive) p.AdjustmentMs = avg - p.DiffMs;

        sb.AppendLine(Table(participants));
        sb.AppendLine($"Promedio de diferencias = ({string.Join(" + ", included.Select(p => N(p.DiffMs)))}) / {included.Count} = {N(avg)} ms");
        if (cfg.MaxDeviationSeconds > 0)
            sb.AppendLine($"Promedio tolerante a fallas: mediana = {N(median)} ms, se excluyen relojes a más de {cfg.MaxDeviationSeconds} s de la mediana" +
                          (alive.Any(p => !p.InAverage) ? $" -> excluidos: {string.Join(", ", alive.Where(p => !p.InAverage).Select(p => p.Name))}" : " -> ninguno excluido"));
        foreach (var p in participants.Where(p => !p.Responded))
            sb.AppendLine($"!! {p.Name} no respondió ({p.Error}): queda fuera del promedio y no recibe ajuste en esta ronda.");

        // ---- 5. Enviar ajustes y aplicar el propio ----
        foreach (var p in alive)
        {
            if (p.IsCoordinator)
            {
                clock.Adjust(TimeSpan.FromMilliseconds(p.AdjustmentMs));
                continue;
            }
            try
            {
                var resp = await http.PostAsJsonAsync($"{p.Url}/api/clock/adjust", new AdjustRequest(_round, p.AdjustmentMs), ct);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                sb.AppendLine($"!! No se pudo enviar el ajuste a {p.Name} ({ex.GetType().Name}).");
            }
        }

        // ---- Verificación: relojes DESPUÉS de sincronizar ----
        sb.AppendLine("Después de la sincronización:");
        var after = new List<(string Name, DateTime Time, double Diff)> { (cfg.Name + " (coord)", clock.Now, 0) };
        foreach (var p in alive.Where(p => !p.IsCoordinator))
        {
            try
            {
                var t0 = clock.Now;
                var r = await http.GetFromJsonAsync<ClockReading>($"{p.Url}/api/clock", ct);
                var t1 = clock.Now;
                var diff = (r!.Time.AddMilliseconds((t1 - t0).TotalMilliseconds / 2) - t1).TotalMilliseconds;
                after.Add((p.Name, r.Time, diff));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }
        }
        foreach (var a in after)
            sb.AppendLine($"   {a.Name,-20} hora {Fmt.Time(a.Time)}   diferencia vs coordinador {N(a.Diff),12} ms");
        var spread = after.Max(a => a.Diff) - after.Min(a => a.Diff);
        sb.AppendLine($"   Dispersión máxima entre relojes: ANTES {N(alive.Max(p => p.DiffMs) - alive.Min(p => p.DiffMs))} ms -> DESPUÉS {N(spread)} ms");

        ExecLog.Write(sb.ToString().TrimEnd());
    }

    private static string Table(List<Participant> ps)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"Participante",-20} {"Hora leída",-13} {"RTT ms",8} {"Dif. vs coord",15} {"¿Promedio?",11} {"Ajuste enviado",16} {"Hora corregida",-14}");
        sb.AppendLine(new string('-', 96));
        foreach (var p in ps)
        {
            if (!p.Responded)
            {
                sb.AppendLine($"{p.Name,-20} {"---",-13} {"---",8} {"SIN RESPUESTA",15} {"no",11} {"---",16} {"---",-14}");
                continue;
            }
            var corrected = p.ReadTime!.Value.AddMilliseconds(p.AdjustmentMs);
            sb.AppendLine($"{p.Name,-20} {Fmt.Time(p.ReadTime.Value),-13} {(p.IsCoordinator ? "-" : p.RttMs.ToString("0.0", CultureInfo.InvariantCulture)),8} " +
                          $"{N(p.DiffMs) + " ms",15} {(p.InAverage ? "sí" : "EXCLUIDO"),11} {N(p.AdjustmentMs) + " ms",16} {Fmt.Time(corrected),-14}");
        }
        return sb.ToString().TrimEnd();
    }

    private static double Median(IEnumerable<double> values)
    {
        var v = values.OrderBy(x => x).ToArray();
        if (v.Length == 0) return 0;
        return v.Length % 2 == 1 ? v[v.Length / 2] : (v[v.Length / 2 - 1] + v[v.Length / 2]) / 2;
    }

    private static string N(double ms) => (ms >= 0 ? "+" : "") + ms.ToString("0.0", CultureInfo.InvariantCulture);
}
