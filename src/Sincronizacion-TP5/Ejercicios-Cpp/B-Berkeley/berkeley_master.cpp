// ============================================================
//  Ejercicio B - Algoritmo de Berkeley: computadora MAESTRA
//
//  En cada ronda:
//    1. Pide la hora a cada esclavo y mide el RTT (compensa con RTT/2).
//    2. Calcula la diferencia de cada reloj respecto del suyo (el suyo = 0).
//    3. Promedia las diferencias, incluyendo la suya. Con --max-dev-s > 0
//       descarta del promedio los relojes que se alejan de la mediana
//       (promedio tolerante a fallas).
//    4. Ajuste de cada uno = promedio - diferencia.
//    5. Envía a cada esclavo SU ajuste y corrige su propio reloj.
//    6. Repite cada --interval segundos.
//
//  Uso: berkeley_master --slaves 127.0.0.1:7001,127.0.0.1:7002,127.0.0.1:7003
//  Opciones: --name maestro --offset-s 0 --interval 10 --rounds 0 (0 = infinito)
//            --max-dev-s 0 --timeout-ms 2000 --log-dir logs
// ============================================================
#include <algorithm>
#include <cmath>
#include <numeric>

#include "../common/net.hpp"
#include "../common/util.hpp"

struct Participant {
    std::string name, host;
    int port = 0;
    bool is_master = false;
    bool responded = false;
    int64_t read_ms = 0;   // hora informada
    int64_t rtt_ms = 0;
    int64_t diff_ms = 0;   // diferencia respecto del maestro
    bool in_average = false;
    int64_t adjust_ms = 0;
};

static std::string pad(const std::string& s, int w) {
    return s.size() >= static_cast<size_t>(w) ? s : s + std::string(w - s.size(), ' ');
}
static std::string rpad(const std::string& s, int w) {
    return s.size() >= static_cast<size_t>(w) ? s : std::string(w - s.size(), ' ') + s;
}

// Lee la hora de un esclavo y calcula su diferencia con el maestro.
static bool poll(Participant& p, SimulatedClock& clock, int timeout_ms) {
    int64_t t0 = clock.now_ms();
    std::string reply;
    if (!net::request(p.host, p.port, "TIME", reply, timeout_ms)) return false;
    int64_t t1 = clock.now_ms();
    std::istringstream in(reply);
    std::string tag;
    long long t;
    if (!(in >> tag >> t) || tag != "TIME") return false;
    p.read_ms = t;
    p.rtt_ms = t1 - t0;
    p.diff_ms = (t + p.rtt_ms / 2) - t1;  // hora del esclavo estimada en t1 menos la del maestro
    return true;
}

int main(int argc, char** argv) {
    net::Init winsock;
    Args args(argc, argv);
    std::string name = args.str("name", "maestro");
    int interval = static_cast<int>(args.num("interval", 10));
    int rounds = static_cast<int>(args.num("rounds", 0));
    double max_dev_s = args.real("max-dev-s", 0);
    int timeout_ms = static_cast<int>(args.num("timeout-ms", 2000));
    logger().init(name, args.str("log-dir", "logs"));
    SimulatedClock clock(static_cast<int64_t>(args.real("offset-s", 0) * 1000), args.real("drift-ppm", 0));

    std::vector<Participant> slaves;
    int k = 1;
    for (auto& hp : split(args.str("slaves", "127.0.0.1:7001,127.0.0.1:7002,127.0.0.1:7003"), ',')) {
        Participant p;
        net::split_host_port(hp, p.host, p.port);
        p.name = "pc-" + std::to_string(k++) + " (" + hp + ")";
        slaves.push_back(p);
    }

    log("Maestro '" + name + "' iniciado | hora " + fmt_time(clock.now_ms()) + " | esclavos: " + std::to_string(slaves.size()) +
        " | intervalo " + std::to_string(interval) + " s | promedio " +
        (max_dev_s > 0 ? "tolerante a fallas (desvio max " + std::to_string(static_cast<int>(max_dev_s)) + " s)" : "simple"));
    std::this_thread::sleep_for(std::chrono::milliseconds(static_cast<int>(args.num("startup-ms", 1500))));

    for (int round = 1; rounds == 0 || round <= rounds; ++round) {
        std::ostringstream o;
        o << std::string(100, '=') << "\nRONDA " << round << "\n";

        // ---- 1 y 2. Leer relojes y calcular diferencias ----
        std::vector<Participant> ps;
        Participant me;
        me.name = name + " (maestro)";
        me.is_master = me.responded = true;
        me.read_ms = clock.now_ms();
        ps.push_back(me);
        for (auto s : slaves) {
            s.responded = poll(s, clock, timeout_ms);
            ps.push_back(s);
        }

        std::vector<Participant*> alive;
        for (auto& p : ps)
            if (p.responded) alive.push_back(&p);

        // ---- 3. Promedio (simple o tolerante a fallas) ----
        std::vector<int64_t> diffs;
        for (auto* p : alive) diffs.push_back(p->diff_ms);
        std::sort(diffs.begin(), diffs.end());
        double median = diffs.size() % 2 ? diffs[diffs.size() / 2]
                                         : (diffs[diffs.size() / 2 - 1] + diffs[diffs.size() / 2]) / 2.0;
        std::vector<int64_t> included;
        for (auto* p : alive) {
            p->in_average = max_dev_s <= 0 || std::abs(p->diff_ms - median) <= max_dev_s * 1000;
            if (p->in_average) included.push_back(p->diff_ms);
        }
        int64_t avg = static_cast<int64_t>(std::llround(
            std::accumulate(included.begin(), included.end(), 0.0) / static_cast<double>(included.size())));

        // ---- 4. Ajuste de cada participante ----
        for (auto* p : alive) p->adjust_ms = avg - p->diff_ms;

        o << pad("Computadora", 26) << pad("Hora leida", 14) << rpad("RTT", 7) << rpad("Diferencia", 15) << rpad("Promedio?", 11)
          << rpad("Ajuste", 13) << "\n" << std::string(86, '-') << "\n";
        for (auto& p : ps) {
            if (!p.responded) {
                o << pad(p.name, 26) << pad("---", 14) << rpad("---", 7) << rpad("SIN RESPUESTA", 15) << rpad("no", 11)
                  << rpad("---", 13) << "\n";
                continue;
            }
            o << pad(p.name, 26) << pad(fmt_time(p.read_ms), 14) << rpad(p.is_master ? "-" : std::to_string(p.rtt_ms), 7)
              << rpad(fmt_ms(p.diff_ms), 15) << rpad(p.in_average ? "si" : "EXCLUIDO", 11) << rpad(fmt_ms(p.adjust_ms), 13) << "\n";
        }
        o << "Promedio = (";
        for (size_t i = 0; i < included.size(); ++i) o << (i ? " + " : "") << fmt_ms(included[i]);
        o << ") / " << included.size() << " = " << fmt_ms(avg) << "\n";
        for (auto& p : ps)
            if (!p.responded) o << "!! " << p.name << " no respondio: queda fuera de esta ronda.\n";
        for (auto* p : alive)
            if (!p->in_average) o << "!! " << p->name << " se aleja mas de " << max_dev_s << " s de la mediana: no entra en el promedio, pero igual se corrige.\n";

        // ---- 5. Enviar ajustes ----
        for (auto* p : alive) {
            if (p->is_master) { clock.adjust(p->adjust_ms); continue; }
            std::string reply;
            if (!net::request(p->host, p->port, "ADJUST " + std::to_string(p->adjust_ms), reply, timeout_ms))
                o << "!! No se pudo enviar el ajuste a " << p->name << "\n";
        }

        // ---- Verificación: relojes después de sincronizar ----
        int64_t min_d = 0, max_d = 0, before_spread = 0;
        {
            int64_t mn = 0, mx = 0;
            for (auto* p : alive) { mn = std::min(mn, p->diff_ms); mx = std::max(mx, p->diff_ms); }
            before_spread = mx - mn;
        }
        o << "Despues de sincronizar:\n";
        o << "   " << pad(name + " (maestro)", 26) << fmt_time(clock.now_ms()) << "   diferencia +0 ms\n";
        for (auto* p : alive) {
            if (p->is_master) continue;
            Participant check = *p;
            if (poll(check, clock, timeout_ms)) {
                o << "   " << pad(check.name, 26) << fmt_time(check.read_ms) << "   diferencia " << fmt_ms(check.diff_ms) << "\n";
                min_d = std::min(min_d, check.diff_ms);
                max_d = std::max(max_d, check.diff_ms);
            }
        }
        o << "   Dispersion entre relojes: ANTES " << before_spread << " ms -> DESPUES " << (max_d - min_d) << " ms";
        log(o.str());

        // ---- 6. Repetir ----
        if (rounds != 0 && round >= rounds) break;
        std::this_thread::sleep_for(std::chrono::seconds(interval));
    }
    log("Maestro finalizado.");
    return 0;
}
