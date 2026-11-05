// ============================================================
//  Ejercicio A - Algoritmo de Lamport
//
//  4 computadoras ejecutan cada una un proceso Pi. La aplicación debe
//  ejecutarse en el orden P1 -> P2 -> P3 -> P4. Cada proceso solo ejecuta
//  su parte cuando recibe el mensaje del anterior, y todos los mensajes
//  llevan la marca de tiempo de Lamport del emisor.
//
//  Reglas del reloj lógico:
//    - evento interno:  L = L + 1
//    - envío:           L = L + 1, el mensaje lleva L
//    - recepción:       L = max(L, L_mensaje) + 1
//
//  Uso (un proceso por computadora / terminal):
//    lamport --id 1 --port 6001 --next 127.0.0.1:6002
//    lamport --id 2 --port 6002 --next 127.0.0.1:6003
//    lamport --id 3 --port 6003 --next 127.0.0.1:6004
//    lamport --id 4 --port 6004 --next 127.0.0.1:6001
//  Opciones: --n 4 --rounds 2 --offset-s 0 --initial-events 0 --task-ms 400 --log-dir logs
// ============================================================
#include <condition_variable>
#include <queue>
#include <random>

#include "../common/net.hpp"
#include "../common/util.hpp"

struct Event {
    std::string type;         // INTERNO / ENVÍO / RECEPCIÓN
    std::string description;
    int64_t physical_ms;
    long long lamport;
    std::string rule;
};

struct Message {
    int from;
    long long lamport;
    int round;
    std::string text;
};

class LamportProcess {
public:
    LamportProcess(int id, SimulatedClock& clock) : id_(id), clock_(clock) {}

    long long internal(const std::string& desc) {
        std::lock_guard<std::mutex> lk(m_);
        long long before = L_;
        L_ += 1;
        record("INTERNO", desc, std::to_string(before) + "+1");
        return L_;
    }

    long long send(const std::string& desc) {
        std::lock_guard<std::mutex> lk(m_);
        long long before = L_;
        L_ += 1;
        record("ENVIO", desc, std::to_string(before) + "+1");
        return L_;
    }

    long long receive(const Message& msg, const std::string& desc) {
        std::lock_guard<std::mutex> lk(m_);
        long long before = L_;
        L_ = std::max(L_, msg.lamport) + 1;
        std::string rule = "max(" + std::to_string(before) + "," + std::to_string(msg.lamport) + ")+1";
        if (msg.lamport > before) rule += "  <- se adelanta";
        record("RECEPCION", desc, rule);
        return L_;
    }

    void print_summary() {
        std::lock_guard<std::mutex> lk(m_);
        std::ostringstream o;
        o << "\nRESUMEN DE EVENTOS DE P" << id_ << "\n";
        o << header() << "\n";
        for (auto& e : events_) o << row(e) << "\n";
        log(o.str());
    }

    static std::string header() {
        char buf[256];
        std::snprintf(buf, sizeof(buf), "| %-10s| %-46s| %-13s| %7s | %-30s|", "Tipo", "Evento", "Reloj fisico", "Lamport", "Regla");
        return std::string(buf) + "\n|" + std::string(11, '-') + "|" + std::string(47, '-') + "|" + std::string(14, '-') +
               "|" + std::string(9, '-') + "|" + std::string(31, '-') + "|";
    }

private:
    std::string row(const Event& e) const {
        char buf[320];
        std::snprintf(buf, sizeof(buf), "| %-10s| %-46s| %-13s| %7lld | %-30s|", e.type.c_str(),
                      e.description.substr(0, 46).c_str(), fmt_time(e.physical_ms).c_str(), e.lamport, e.rule.c_str());
        return buf;
    }

    void record(const std::string& type, const std::string& desc, const std::string& rule) {
        Event e{type, desc, clock_.now_ms(), L_, rule};
        events_.push_back(e);
        log(row(e));
    }

    int id_;
    SimulatedClock& clock_;
    std::mutex m_;
    long long L_ = 0;
    std::vector<Event> events_;
};

// Cola de mensajes recibidos (la llena el hilo que escucha el puerto).
class Inbox {
public:
    void push(const Message& m) {
        { std::lock_guard<std::mutex> lk(m_); q_.push(m); }
        cv_.notify_one();
    }
    Message pop() {
        std::unique_lock<std::mutex> lk(m_);
        cv_.wait(lk, [&] { return !q_.empty(); });
        Message m = q_.front();
        q_.pop();
        return m;
    }

private:
    std::mutex m_;
    std::condition_variable cv_;
    std::queue<Message> q_;
};

// Formato en la red: "MSG <emisor> <lamport> <ronda> <texto>"
static bool parse_message(const std::string& line, Message& m) {
    std::istringstream in(line);
    std::string tag;
    if (!(in >> tag >> m.from >> m.lamport >> m.round) || tag != "MSG") return false;
    std::getline(in, m.text);
    if (!m.text.empty() && m.text[0] == ' ') m.text.erase(0, 1);
    return true;
}

int main(int argc, char** argv) {
    net::Init winsock;
    Args args(argc, argv);
    int id = static_cast<int>(args.num("id", 1));
    int n = static_cast<int>(args.num("n", 4));
    int port = static_cast<int>(args.num("port", 6000 + id));
    int rounds = static_cast<int>(args.num("rounds", 2));
    int initial_events = static_cast<int>(args.num("initial-events", 0));
    int task_ms = static_cast<int>(args.num("task-ms", 400));
    int next_id = id % n + 1;
    std::string next_host;
    int next_port;
    net::split_host_port(args.str("next", "127.0.0.1:" + std::to_string(6000 + next_id)), next_host, next_port);

    logger().init("P" + std::to_string(id), args.str("log-dir", "logs"));
    SimulatedClock clock(static_cast<int64_t>(args.real("offset-s", 0) * 1000));
    LamportProcess proc(id, clock);
    Inbox inbox;

    log("Proceso P" + std::to_string(id) + " iniciado en el puerto " + std::to_string(port) +
        " | reloj fisico " + fmt_time(clock.now_ms()) + " (desfase " + fmt_ms(clock.offset_ms()) + ")" +
        " | siguiente: P" + std::to_string(next_id) + " en " + next_host + ":" + std::to_string(next_port));
    log("Orden de la aplicacion: P1 -> P2 -> ... -> P" + std::to_string(n) + " (" + std::to_string(rounds) + " ronda/s)\n" +
        LamportProcess::header());

    // Hilo que escucha: recibe mensajes y los encola.
    socket_t server = net::listen_on(port);
    std::thread([&] {
        while (true) {
            socket_t c = net::accept_client(server);
            if (c == NET_INVALID) continue;
            std::string line;
            Message m;
            if (net::recv_line(c, line) && parse_message(line, m)) {
                net::send_line(c, "ACK");
                inbox.push(m);
            }
            net::close(c);
        }
    }).detach();

    // Eventos locales previos (hacen que los contadores arranquen distintos).
    for (int k = 1; k <= initial_events; ++k) proc.internal("Evento local previo #" + std::to_string(k));

    std::mt19937 rng(static_cast<unsigned>(id * 7919));
    auto deliver = [&](int round, const std::string& text) {
        long long ts = proc.send("Envia a P" + std::to_string(next_id) + ": " + text);
        std::string msg = "MSG " + std::to_string(id) + " " + std::to_string(ts) + " " + std::to_string(round) + " " + text;
        while (true) {
            socket_t s = net::connect_retry(next_host, next_port);
            if (s == NET_INVALID) { log("!! No se pudo contactar a P" + std::to_string(next_id) + ", reintentando..."); continue; }
            net::set_recv_timeout(s, 5000);
            std::string ack;
            bool ok = net::send_line(s, msg) && net::recv_line(s, ack) && ack == "ACK";
            net::close(s);
            if (ok) return;
        }
    };

    for (int r = 1; r <= rounds; ++r) {
        // P1 arranca la primera ronda; en las demás, y en el resto de los procesos,
        // se espera el mensaje del proceso anterior antes de ejecutar.
        if (id != 1 || r > 1) {
            Message m = inbox.pop();
            proc.receive(m, "Recibe de P" + std::to_string(m.from) + " (L=" + std::to_string(m.lamport) + "): " + m.text);
        }
        proc.internal("Ejecuta su parte de la aplicacion (ronda " + std::to_string(r) + ")");
        std::this_thread::sleep_for(std::chrono::milliseconds(task_ms + static_cast<int>(rng() % 200)));
        std::string text = id == n ? "fin de ronda " + std::to_string(r) : "P" + std::to_string(id) + " termino, sigue P" + std::to_string(next_id);
        deliver(r, text);
    }

    if (id == 1) {
        Message m = inbox.pop();
        proc.receive(m, "Recibe de P" + std::to_string(m.from) + " (L=" + std::to_string(m.lamport) + "): " + m.text);
        proc.internal("Aplicacion completada en orden P1..P" + std::to_string(n));
    }

    proc.print_summary();
    net::close(server);
    return 0;
}
