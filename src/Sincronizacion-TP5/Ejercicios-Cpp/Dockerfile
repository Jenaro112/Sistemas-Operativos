# Una sola imagen con los 5 programas de los ejercicios A, B y C.
FROM gcc:13 AS build
WORKDIR /src
COPY common/ common/
COPY A-Lamport/*.cpp A-Lamport/
COPY B-Berkeley/*.cpp B-Berkeley/
COPY C-Cristian/*.cpp C-Cristian/
RUN mkdir -p /out && for f in A-Lamport/lamport B-Berkeley/berkeley_master B-Berkeley/berkeley_slave \
        C-Cristian/cristian_server C-Cristian/cristian_client; do \
      g++ -std=c++17 -O2 -pthread -static-libstdc++ -static-libgcc -o /out/$(basename $f) $f.cpp || exit 1; \
    done

FROM debian:bookworm-slim
WORKDIR /app
COPY --from=build /out/ /usr/local/bin/
ENV DISPLAY_UTC_OFFSET_HOURS=-3
