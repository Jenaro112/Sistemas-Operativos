// ============================================================
//  Ejercicio B - Algoritmo de Berkeley: computadora ESCLAVA
//
//  Atiende dos pedidos del maestro:
//    "TIME"            -> responde "TIME <hora_ms>"
//    "ADJUST <ms>"     -> corrige su reloj y responde "OK <hora_ms>"
//
//  Uso: berkeley_slave --name pc-1 --port 7001 --offset-s 5 --drift-ppm 300
// ============================================================
#include "../common/net.hpp"
#include "../common/util.hpp"

int main(int argc, char** argv) {
    net::Init winsock;
    Args args(argc, argv);
    std::string name = args.str("name", "esclavo");
    int port = static_cast<int>(args.num("port", 7001));
    logger().init(name, args.str("log-dir", "logs"));
    SimulatedClock clock(static_cast<int64_t>(args.real("offset-s", 0) * 1000), args.real("drift-ppm", 0));

    log("Esclavo '" + name + "' escuchando en el puerto " + std::to_string(port) + " | hora " + fmt_time(clock.now_ms()) +
        " (desfase " + fmt_ms(clock.offset_ms()) + ", deriva " + std::to_string(static_cast<int>(clock.drift_ppm())) + " ppm)");

    socket_t server = net::listen_on(port);
    while (true) {
        socket_t c = net::accept_client(server);
        if (c == NET_INVALID) continue;
        std::string line;
        if (net::recv_line(c, line)) {
            std::istringstream in(line);
            std::string cmd;
            in >> cmd;
            if (cmd == "TIME") {
                int64_t now = clock.now_ms();
                net::send_line(c, "TIME " + std::to_string(now));
                log("El maestro pide la hora -> " + fmt_time(now));
            } else if (cmd == "ADJUST") {
                long long delta = 0;
                in >> delta;
                int64_t before = clock.now_ms();
                clock.adjust(delta);
                int64_t after = clock.now_ms();
                net::send_line(c, "OK " + std::to_string(after));
                log("Ajuste recibido " + fmt_ms(delta) + " | antes " + fmt_time(before) + " -> despues " + fmt_time(after) +
                    " | desfase acumulado " + fmt_ms(clock.offset_ms()));
            } else {
                net::send_line(c, "ERROR comando desconocido");
            }
        }
        net::close(c);
    }
}
