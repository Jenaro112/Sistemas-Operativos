// ============================================================
//  Ejercicio C - Método de Cristian: SERVIDOR DE TIEMPO
//
//  Responde "TIME" con "TIME <hora_ms>".
//  Opciones para las pruebas:
//    --req-delay-ms  demora simulada del viaje de ida (antes de leer el reloj)
//    --resp-delay-ms demora simulada del viaje de vuelta (después de leer el reloj)
//
//  Uso: cristian_server --port 8001 --offset-s 2
// ============================================================
#include "../common/net.hpp"
#include "../common/util.hpp"

int main(int argc, char** argv) {
    net::Init winsock;
    Args args(argc, argv);
    int port = static_cast<int>(args.num("port", 8001));
    int req_delay = static_cast<int>(args.num("req-delay-ms", 0));
    int resp_delay = static_cast<int>(args.num("resp-delay-ms", 0));
    logger().init(args.str("name", "servidor-tiempo"), args.str("log-dir", "logs"));
    SimulatedClock clock(static_cast<int64_t>(args.real("offset-s", 0) * 1000));

    log("Servidor de tiempo en el puerto " + std::to_string(port) + " | hora " + fmt_time(clock.now_ms()) + " (desfase " +
        fmt_ms(clock.offset_ms()) + ") | red simulada: ida " + std::to_string(req_delay) + " ms, vuelta " +
        std::to_string(resp_delay) + " ms");

    socket_t server = net::listen_on(port);
    long long n = 0;
    while (true) {
        socket_t c = net::accept_client(server);
        if (c == NET_INVALID) continue;
        // Cada pedido en su propio hilo, así un cliente lento no frena a los demás.
        std::thread([c, &clock, req_delay, resp_delay, id = ++n] {
            std::string line;
            if (net::recv_line(c, line)) {
                std::istringstream in(line);
                std::string cmd, who;
                in >> cmd >> who;
                if (cmd == "TIME") {
                    std::this_thread::sleep_for(std::chrono::milliseconds(req_delay));
                    int64_t now = clock.now_ms();
                    log("#" + std::to_string(id) + " pedido de '" + (who.empty() ? "?" : who) + "' -> " + fmt_time(now));
                    std::this_thread::sleep_for(std::chrono::milliseconds(resp_delay));
                    net::send_line(c, "TIME " + std::to_string(now));
                }
            }
            net::close(c);
        }).detach();
    }
}
