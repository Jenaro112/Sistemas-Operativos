// ============================================================
//  Ejercicio C - Método de Cristian: CLIENTE
//
//    1. T0 = hora local al enviar el pedido
//    2. Ts = hora que devuelve el servidor
//    3. T1 = hora local al recibir la respuesta
//    4. RTT = T1 - T0
//    5. Hora estimada en T1 = Ts + RTT / 2
//    6. Corrección = hora estimada - T1
//
//  El reloj del sistema NO se modifica: se corrige un reloj lógico de la
//  aplicación (hora local + corrección). Se toman varias muestras por ronda
//  y se usa la de menor RTT; las que superan --max-rtt-ms se descartan.
//
//  Uso: cristian_client --name pc-A --server 127.0.0.1:8001 --offset-s 8
//  Opciones: --drift-ppm 0 --samples 3 --interval 5 --rounds 0 (0 = infinito)
//            --max-rtt-ms 1000 --timeout-ms 2000 --log-dir logs
// ============================================================
#include <cmath>

#include "../common/net.hpp"
#include "../common/util.hpp"

struct Sample {
    int n;
    int64_t t0, t1, ts;
    double rtt_ms;
};

int main(int argc, char** argv) {
    net::Init winsock;
    Args args(argc, argv);
    std::string name = args.str("name", "cliente");
    std::string host;
    int port;
    net::split_host_port(args.str("server", "127.0.0.1:8001"), host, port);
    int samples = static_cast<int>(args.num("samples", 3));
    int interval = static_cast<int>(args.num("interval", 5));
    int rounds = static_cast<int>(args.num("rounds", 0));
    int max_rtt = static_cast<int>(args.num("max-rtt-ms", 1000));
    int timeout_ms = static_cast<int>(args.num("timeout-ms", 2000));
    logger().init(name, args.str("log-dir", "logs"));

    SimulatedClock local(static_cast<int64_t>(args.real("offset-s", 0) * 1000), args.real("drift-ppm", 0));
    int64_t correction = 0;  // reloj de la app = reloj local + corrección

    log("Cliente '" + name + "' | hora local " + fmt_time(local.now_ms()) + " (desfase " + fmt_ms(local.offset_ms()) +
        ", deriva " + std::to_string(static_cast<int>(local.drift_ppm())) + " ppm) | servidor " + host + ":" +
        std::to_string(port) + " | " + std::to_string(samples) + " muestras por ronda, RTT max " + std::to_string(max_rtt) + " ms");
    std::this_thread::sleep_for(std::chrono::milliseconds(static_cast<int>(args.num("startup-ms", 1000))));

    for (int round = 1; rounds == 0 || round <= rounds; ++round) {
        std::ostringstream o;
        o << std::string(72, '=') << "\nRONDA " << round << "\n";
        std::vector<Sample> got;

        // ---- 1 a 4: pedir la hora y medir el RTT ----
        for (int i = 1; i <= samples; ++i) {
            int64_t t0 = local.now_ms();
            auto start = std::chrono::steady_clock::now();
            std::string reply;
            bool ok = net::request(host, port, "TIME " + name, reply, timeout_ms);
            double rtt = std::chrono::duration<double, std::milli>(std::chrono::steady_clock::now() - start).count();
            int64_t t1 = local.now_ms();
            std::istringstream in(reply);
            std::string tag;
            long long ts = 0;
            if (!ok || !(in >> tag >> ts) || tag != "TIME") {
                o << "  Muestra " << i << ": SIN RESPUESTA tras " << static_cast<int>(rtt) << " ms\n";
                continue;
            }
            got.push_back({i, t0, t1, ts, rtt});
            char buf[160];
            std::snprintf(buf, sizeof(buf), "  Muestra %d: T0=%s  T1=%s  Ts=%s  RTT=%.2f ms\n", i, fmt_time(t0).c_str(),
                          fmt_time(t1).c_str(), fmt_time(ts).c_str(), rtt);
            o << buf;
        }

        // ---- Falla: el servidor no responde ----
        if (got.empty()) {
            o << "  !! El servidor de tiempo NO RESPONDE. Se mantiene la ultima correccion (" << fmt_ms(correction)
              << ") y el reloj sigue con su deriva.\n  Hora app: " << fmt_time(local.now_ms() + correction);
            log(o.str());
        } else {
            // ---- Elegir la muestra con menor RTT ----
            const Sample* best = nullptr;
            for (auto& s : got)
                if (s.rtt_ms <= max_rtt && (!best || s.rtt_ms < best->rtt_ms)) best = &s;
            if (!best) {
                o << "  !! Todas las muestras superan el RTT maximo (" << max_rtt << " ms): NO se ajusta el reloj.";
                log(o.str());
            } else {
                // ---- 5. Estimar la hora: Ts + RTT/2 ----
                int64_t half = static_cast<int64_t>(std::llround(best->rtt_ms / 2));
                int64_t estimated = best->ts + half;
                // ---- 6. Ajustar el reloj lógico de la aplicación ----
                int64_t before = best->t1 + correction;
                correction = estimated - best->t1;
                int64_t after = best->t1 + correction;
                char buf[64], prec[64];
                std::snprintf(buf, sizeof(buf), "%.2f", best->rtt_ms);
                std::snprintf(prec, sizeof(prec), "%.2f", best->rtt_ms / 2);
                o << "  Muestra elegida: #" << best->n << " (RTT minimo " << buf << " ms)\n"
                  << "  Hora estimada = Ts + RTT/2 = " << fmt_time(best->ts) << " + " << half << " ms = " << fmt_time(estimated) << "\n"
                  << "  Reloj del sistema (sin tocar): " << fmt_time(best->t1) << "\n"
                  << "  Reloj de la app ANTES       : " << fmt_time(before) << "\n"
                  << "  Reloj de la app DESPUES     : " << fmt_time(after) << "\n"
                  << "  Diferencia aplicada         : " << fmt_ms(after - before) << "\n"
                  << "  Correccion total vs sistema : " << fmt_ms(correction) << "\n"
                  << "  Precision estimada          : +/-" << prec << " ms";
                log(o.str());
            }
        }

        if (rounds != 0 && round >= rounds) break;
        std::this_thread::sleep_for(std::chrono::seconds(interval));
    }
    log("Cliente finalizado. Hora sistema " + fmt_time(local.now_ms()) + " | hora app " + fmt_time(local.now_ms() + correction));
    return 0;
}
