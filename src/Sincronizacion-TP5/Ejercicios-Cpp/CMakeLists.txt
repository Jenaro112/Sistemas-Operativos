cmake_minimum_required(VERSION 3.16)
project(SincronizacionCpp CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
find_package(Threads REQUIRED)

function(add_tp_exe name src)
    add_executable(${name} ${src})
    target_link_libraries(${name} PRIVATE Threads::Threads)
    if (WIN32)
        target_link_libraries(${name} PRIVATE ws2_32)
    endif()
endfunction()

# A - Lamport
add_tp_exe(lamport          A-Lamport/lamport.cpp)
# B - Berkeley
add_tp_exe(berkeley_master  B-Berkeley/berkeley_master.cpp)
add_tp_exe(berkeley_slave   B-Berkeley/berkeley_slave.cpp)
# C - Cristian
add_tp_exe(cristian_server  C-Cristian/cristian_server.cpp)
add_tp_exe(cristian_client  C-Cristian/cristian_client.cpp)
