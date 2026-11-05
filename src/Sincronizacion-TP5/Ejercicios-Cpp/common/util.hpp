// ============================================================
//  util.hpp - Reloj físico simulado, formato de horas, logs y argumentos
// ============================================================
#pragma once

#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <map>
#include <mutex>
#include <sstream>
#include <string>
#include <vector>

// ------------------------------------------------------------
// Reloj físico SIMULADO: no toca el reloj del sistema operativo.
//   ahora = horaInicio + transcurrido * (1 + deriva_ppm / 1e6) + desfase
// Permite que varios procesos en la misma PC tengan relojes distintos.
// ------------------------------------------------------------
class SimulatedClock {
public:
    SimulatedClock(int64_t offset_ms = 0, double drift_ppm = 0)
        : offset_ms_(offset_ms), drift_factor_(1.0 + drift_ppm / 1e6), drift_ppm_(drift_ppm) {
        using namespace std::chrono;
        real_start_ms_ = duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count();
        steady_start_ = steady_clock::now();
    }

    // Hora actual de este reloj, en ms desde 1970 (UTC).
    int64_t now_ms() const {
        using namespace std::chrono;
        std::lock_guard<std::mutex> lk(m_);
        double elapsed_us = static_cast<double>(duration_cast<microseconds>(steady_clock::now() - steady_start_).count());
        return real_start_ms_ + static_cast<int64_t>(elapsed_us * drift_factor_ / 1000.0) + offset_ms_;
    }

    // Adelanta (delta > 0) o atrasa (delta < 0) el reloj.
    void adjust(int64_t delta_ms) {
        std::lock_guard<std::mutex> lk(m_);
        offset_ms_ += delta_ms;
    }

    int64_t offset_ms() const {
        std::lock_guard<std::mutex> lk(m_);
        return offset_ms_;
    }

    double drift_ppm() const { return drift_ppm_; }

private:
    mutable std::mutex m_;
    int64_t real_start_ms_{};
    std::chrono::steady_clock::time_point steady_start_;
    int64_t offset_ms_;
    double drift_factor_;
    double drift_ppm_;
};

// Hora del sistema operativo en ms (para medir el RTT con precisión).
inline int64_t steady_ms() {
    using namespace std::chrono;
    return duration_cast<milliseconds>(steady_clock::now().time_since_epoch()).count();
}

// HH:MM:SS.mmm en la zona horaria de visualización (UTC-3 por defecto, variable DISPLAY_UTC_OFFSET_HOURS).
inline std::string fmt_time(int64_t epoch_ms) {
    static const int64_t offset_h = [] {
        const char* v = std::getenv("DISPLAY_UTC_OFFSET_HOURS");
        return v ? std::atoll(v) : -3LL;
    }();
    int64_t day_ms = 86400000LL;
    int64_t t = ((epoch_ms + offset_h * 3600000LL) % day_ms + day_ms) % day_ms;
    char buf[32];
    std::snprintf(buf, sizeof(buf), "%02lld:%02lld:%02lld.%03lld",
                  static_cast<long long>(t / 3600000), static_cast<long long>(t / 60000 % 60),
                  static_cast<long long>(t / 1000 % 60), static_cast<long long>(t % 1000));
    return buf;
}

// "+1234 ms" / "-56 ms"
inline std::string fmt_ms(int64_t ms) {
    return (ms >= 0 ? "+" : "") + std::to_string(ms) + " ms";
}

// ------------------------------------------------------------
// Log: escribe en consola y en <dir_logs>/<nombre>.log
// ------------------------------------------------------------
class Logger {
public:
    void init(const std::string& name, const std::string& dir) {
        name_ = name;
        if (!dir.empty()) {
            std::filesystem::create_directories(dir);
            file_.open(std::filesystem::path(dir) / (name + ".log"), std::ios::app);
        }
    }

    void write(const std::string& msg) {
        std::lock_guard<std::mutex> lk(m_);
        std::istringstream in(msg);
        std::string line;
        while (std::getline(in, line)) {
            std::string out = "[" + name_ + "] " + line;
            std::cout << out << std::endl;
            if (file_.is_open()) file_ << out << std::endl;
        }
    }

private:
    std::mutex m_;
    std::string name_ = "app";
    std::ofstream file_;
};

inline Logger& logger() {
    static Logger l;
    return l;
}

inline void log(const std::string& msg) { logger().write(msg); }

// ------------------------------------------------------------
// Argumentos del estilo --clave valor
// ------------------------------------------------------------
class Args {
public:
    Args(int argc, char** argv) {
        for (int i = 1; i < argc; ++i) {
            std::string k = argv[i];
            if (k.rfind("--", 0) == 0) {
                k = k.substr(2);
                if (i + 1 < argc && std::string(argv[i + 1]).rfind("--", 0) != 0) values_[k] = argv[++i];
                else values_[k] = "true";
            }
        }
    }
    std::string str(const std::string& k, const std::string& def = "") const {
        auto it = values_.find(k);
        return it == values_.end() ? def : it->second;
    }
    long long num(const std::string& k, long long def) const {
        auto it = values_.find(k);
        return it == values_.end() ? def : std::stoll(it->second);
    }
    double real(const std::string& k, double def) const {
        auto it = values_.find(k);
        return it == values_.end() ? def : std::stod(it->second);
    }
    bool has(const std::string& k) const { return values_.count(k) > 0; }

private:
    std::map<std::string, std::string> values_;
};

inline std::vector<std::string> split(const std::string& s, char sep) {
    std::vector<std::string> out;
    std::string item;
    std::istringstream in(s);
    while (std::getline(in, item, sep))
        if (!item.empty()) out.push_back(item);
    return out;
}
