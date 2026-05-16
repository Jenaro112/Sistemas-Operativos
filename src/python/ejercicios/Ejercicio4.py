import threading

contador_inseguro = 0
contador_sincronizado = 0
lock = threading.Lock()

def incrementar_inseguro():
    global contador_inseguro
    for _ in range(10000):
        contador_inseguro += 1

def incrementar_sincronizado():
    global contador_sincronizado
    for _ in range(10000):
        with lock:
            contador_sincronizado += 1

# SIN sincronizacion
hilos = [threading.Thread(target=incrementar_inseguro) for _ in range(5)]
for h in hilos: h.start()
for h in hilos: h.join()
print(f"Esperado: 50000 | Sin control: {contador_inseguro}")

# CON Lock
hilos = [threading.Thread(target=incrementar_sincronizado) for _ in range(5)]
for h in hilos: h.start()
for h in hilos: h.join()
print(f"Esperado: 50000 | Con Lock: {contador_sincronizado}")

"""
Preguntas:
1. ¿Por qué el resultado puede ser incorrecto?
   Porque += no es atomico: es leer, sumar, escribir. Dos hilos pueden
   leer el mismo valor, incrementarlo y escribir, perdiendose incrementos.

2. ¿Qué es una seccion critica?
   Es la parte del codigo que accede a un recurso compartido y no debe
   ser ejecutada por mas de un hilo a la vez para evitar inconsistencias.

3. ¿Qué diferencia hay entre usar lock y operaciones atomicas?
   - lock: bloquea una seccion critica completa, permitiendo varias operaciones
     como una unidad atomica. Puede causar contention/sobrecarga.
   - atomicas (AtomicInteger, Interlocked): operaciones a nivel de hardware
     (CAS), mas rapidas y sin riesgo de deadlocks, pero solo para
     operaciones simples (incrementar, sumar, etc.).
"""
