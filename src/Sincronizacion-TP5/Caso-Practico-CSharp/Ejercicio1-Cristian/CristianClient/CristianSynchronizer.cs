using System.Net.Http.Json;

namespace CristianClient;

public record TimeResponse(string Server, DateTime ServerTime, double ProcessingMs);

/// <summary>Una medición: solicitud enviada en T0, respuesta recibida en T1 (ambas con el reloj LOCAL).</summary>
public record Sample(int Number, DateTime T0, DateTime T1, DateTime ServerTime, double ProcessingMs)
{
    public double RttMs => (T1 - T0).TotalMilliseconds;
}

/// <summary>
/// Implementación del algoritmo de Cristian:
///
///   1. T0 = hora local al enviar la solicitud
///   2. Ts = hora que devuelve el servidor de tiempo
///   3. T1 = hora local al recibir la respuesta
///   4. RTT = T1 - T0
///   5. Hora estimada en T1 = Ts + (RTT - I) / 2      (I = tiempo de procesamiento del servidor)
///   6. Corrección = Hora estimada - T1
///
/// Se toman varias muestras y se usa la de MENOR RTT (la más precisa).
/// Las muestras con RTT mayor a MAX_RTT_MS se descartan.
/// </summary>
public sealed class CristianSynchronizer(
    ClientConfig cfg, SimulatedClock localClock, ApplicationClock appClock, HttpClient http)
{
    private int _round;
    private int _consecutiveFailures;
    private DateTime? _lastSyncLocal;

    public async Task RunRoundAsync(CancellationToken ct)
    {
        _round++;
        var sep = new string('=', 72);
        ExecLog.Write($"{sep}\nRONDA {_round} - {cfg.Name}");

        // ---------- 1..4: solicitar la hora y medir el RTT ----------
        var samples = new List<Sample>();
        for (var i = 1; i <= cfg.Samples; i++)
        {
            var t0 = localClock.Now;                                    // (2) instante de envío
            try
            {
                var resp = await http.GetFromJsonAsync<TimeResponse>(
                    $"api/time?client={Uri.EscapeDataString(cfg.Name)}", ct);
                var t1 = localClock.Now;                                // (3) instante de recepción
                var s = new Sample(i, t0, t1, resp!.ServerTime, resp.ProcessingMs);
                samples.Add(s);
                ExecLog.Write($"  Muestra {i}: T0={Fmt.Time(t0)}  T1={Fmt.Time(t1)}  " +
                              $"Ts={Fmt.Time(s.ServerTime)}  RTT={s.RttMs:0.0} ms");    // (4) RTT
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                var waited = (localClock.Now - t0).TotalMilliseconds;
                ExecLog.Write($"  Muestra {i}: SIN RESPUESTA tras {waited:0} ms ({ex.GetType().Name})");
            }
        }

        // ---------- Falla: el servidor de tiempo no responde ----------
        if (samples.Count == 0)
        {
            _consecutiveFailures++;
            var since = _lastSyncLocal is null ? "nunca se sincronizó" :
                $"última sincronización hace {(localClock.Now - _lastSyncLocal.Value).TotalSeconds:0} s";
            ExecLog.Write($"  !! Servidor de tiempo NO RESPONDE ({_consecutiveFailures} ronda/s seguidas, {since}).\n" +
                          $"  !! Se mantiene la última corrección ({Fmt.Ms(appClock.Correction)}); el reloj de la app\n" +
                          $"  !! sigue avanzando con la deriva del reloj local ({localClock.DriftPpm} ppm) y el error crece.\n" +
                          $"  Hora app actual: {Fmt.Time(appClock.Now)}");
            return;
        }
        _consecutiveFailures = 0;

        // ---------- Filtrar muestras con RTT excesivo ----------
        var valid = samples.Where(s => s.RttMs <= cfg.MaxRttMs).ToList();
        if (valid.Count == 0)
        {
            ExecLog.Write($"  !! Todas las muestras superan el RTT máximo ({cfg.MaxRttMs} ms). " +
                          $"La estimación sería imprecisa (±{samples.Min(s => s.RttMs) / 2:0.0} ms): NO se ajusta el reloj.");
            return;
        }

        var best = valid.MinBy(s => s.RttMs)!;

        // ---------- 5: estimar la hora con Cristian ----------
        var oneWayMs = (best.RttMs - best.ProcessingMs) / 2;
        var estimated = best.ServerTime.AddMilliseconds(oneWayMs);

        // ---------- 6: ajustar el reloj lógico de la app (NO el del SO) ----------
        var before = appClock.At(best.T1);                // hora app en T1 antes de ajustar
        var newCorrection = estimated - best.T1;          // corrección respecto del reloj local
        appClock.SetCorrection(newCorrection);
        var after = appClock.At(best.T1);                 // == estimated
        _lastSyncLocal = best.T1;

        // ---------- 7: mostrar la diferencia ----------
        var diff = after - before;
        ExecLog.Write(
            $"  Muestra elegida: #{best.Number} (RTT mínimo = {best.RttMs:0.0} ms, procesamiento servidor = {best.ProcessingMs:0.000} ms)\n" +
            $"  Hora estimada = Ts + (RTT - I)/2 = {Fmt.Time(best.ServerTime)} + {oneWayMs:0.0} ms = {Fmt.Time(estimated)}\n" +
            $"  Reloj del SO (no se modifica) : {Fmt.Time(best.T1)}\n" +
            $"  Reloj de la app ANTES         : {Fmt.Time(before)}\n" +
            $"  Reloj de la app DESPUÉS       : {Fmt.Time(after)}\n" +
            $"  Diferencia (después - antes)  : {Fmt.Ms(diff)}\n" +
            $"  Corrección total vs reloj SO  : {Fmt.Ms(newCorrection)}\n" +
            $"  Precisión estimada            : ±{oneWayMs:0.0} ms");
    }
}
