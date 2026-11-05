// ============================================================
//  net.hpp - Sockets TCP mínimos, multiplataforma (Windows / Linux / macOS)
//  Protocolo de texto: cada mensaje es una línea terminada en '\n'.
// ============================================================
#pragma once

#include <chrono>
#include <cstring>
#include <stdexcept>
#include <string>
#include <thread>

#ifdef _WIN32
  #ifndef WIN32_LEAN_AND_MEAN
    #define WIN32_LEAN_AND_MEAN
  #endif
  #include <winsock2.h>
  #include <ws2tcpip.h>
  using socket_t = SOCKET;
  #define NET_INVALID INVALID_SOCKET
  #define NET_CLOSE closesocket
#else
  #include <arpa/inet.h>
  #include <netdb.h>
  #include <netinet/in.h>
  #include <netinet/tcp.h>
  #include <sys/socket.h>
  #include <sys/time.h>
  #include <unistd.h>
  using socket_t = int;
  #define NET_INVALID (-1)
  #define NET_CLOSE ::close
#endif

namespace net {

// En Windows hay que inicializar Winsock una vez por proceso.
struct Init {
    Init() {
#ifdef _WIN32
        WSADATA wsa;
        if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) throw std::runtime_error("WSAStartup falló");
#endif
    }
    ~Init() {
#ifdef _WIN32
        WSACleanup();
#endif
    }
};

inline void close(socket_t s) {
    if (s != NET_INVALID) NET_CLOSE(s);
}

// Timeout de recepción (ms). 0 = sin timeout.
inline void set_recv_timeout(socket_t s, int ms) {
#ifdef _WIN32
    DWORD tv = static_cast<DWORD>(ms);
    setsockopt(s, SOL_SOCKET, SO_RCVTIMEO, reinterpret_cast<const char*>(&tv), sizeof(tv));
#else
    timeval tv{ms / 1000, (ms % 1000) * 1000};
    setsockopt(s, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));
#endif
}

inline void set_nodelay(socket_t s) {
    int one = 1;
    setsockopt(s, IPPROTO_TCP, TCP_NODELAY, reinterpret_cast<const char*>(&one), sizeof(one));
}

// Abre un socket que escucha en todas las interfaces.
inline socket_t listen_on(int port) {
    socket_t s = socket(AF_INET, SOCK_STREAM, 0);
    if (s == NET_INVALID) throw std::runtime_error("no se pudo crear el socket");
    int one = 1;
    setsockopt(s, SOL_SOCKET, SO_REUSEADDR, reinterpret_cast<const char*>(&one), sizeof(one));
    sockaddr_in addr{};
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = htonl(INADDR_ANY);
    addr.sin_port = htons(static_cast<unsigned short>(port));
    if (bind(s, reinterpret_cast<sockaddr*>(&addr), sizeof(addr)) != 0)
        throw std::runtime_error("no se pudo usar el puerto " + std::to_string(port));
    if (listen(s, 16) != 0) throw std::runtime_error("listen falló");
    return s;
}

inline socket_t accept_client(socket_t server) {
    socket_t c = accept(server, nullptr, nullptr);
    if (c != NET_INVALID) set_nodelay(c);
    return c;
}

// Conecta a host:port. Devuelve NET_INVALID si no pudo.
inline socket_t connect_once(const std::string& host, int port) {
    addrinfo hints{};
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    addrinfo* res = nullptr;
    if (getaddrinfo(host.c_str(), std::to_string(port).c_str(), &hints, &res) != 0 || !res)
        return NET_INVALID;
    socket_t s = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (s != NET_INVALID && ::connect(s, res->ai_addr, static_cast<int>(res->ai_addrlen)) != 0) {
        close(s);
        s = NET_INVALID;
    }
    freeaddrinfo(res);
    if (s != NET_INVALID) set_nodelay(s);
    return s;
}

// Reintenta la conexión (útil si el otro proceso todavía no arrancó).
inline socket_t connect_retry(const std::string& host, int port, int attempts = 60, int wait_ms = 500) {
    for (int i = 0; i < attempts; ++i) {
        socket_t s = connect_once(host, port);
        if (s != NET_INVALID) return s;
        std::this_thread::sleep_for(std::chrono::milliseconds(wait_ms));
    }
    return NET_INVALID;
}

inline bool send_line(socket_t s, const std::string& line) {
    std::string data = line + "\n";
    const char* p = data.data();
    size_t left = data.size();
    while (left > 0) {
        auto n = send(s, p, static_cast<int>(left), 0);
        if (n <= 0) return false;
        p += n;
        left -= static_cast<size_t>(n);
    }
    return true;
}

// Lee hasta '\n'. Devuelve false si se cerró la conexión o venció el timeout.
inline bool recv_line(socket_t s, std::string& out) {
    out.clear();
    char c;
    while (true) {
        auto n = recv(s, &c, 1, 0);
        if (n <= 0) return false;
        if (c == '\n') return true;
        if (c != '\r') out.push_back(c);
    }
}

// Separa "host:puerto".
inline void split_host_port(const std::string& hp, std::string& host, int& port) {
    auto pos = hp.rfind(':');
    if (pos == std::string::npos) throw std::runtime_error("se esperaba host:puerto, llegó '" + hp + "'");
    host = hp.substr(0, pos);
    port = std::stoi(hp.substr(pos + 1));
}

// Pedido/respuesta en una conexión nueva: conecta, envía una línea, espera una línea.
// Devuelve false si no hubo respuesta dentro de timeout_ms.
inline bool request(const std::string& host, int port, const std::string& msg, std::string& reply, int timeout_ms) {
    socket_t s = connect_once(host, port);
    if (s == NET_INVALID) return false;
    set_recv_timeout(s, timeout_ms);
    bool ok = send_line(s, msg) && recv_line(s, reply);
    close(s);
    return ok;
}

}  // namespace net
