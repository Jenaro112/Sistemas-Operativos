import threading
import time
from collections import deque

# 1. Mutex / Lock
print("=== 1. Mutex / Lock ===")
lock = threading.Lock()
contador = 0

def incrementar():
    global contador
    for _ in range(1000):
        with lock:
            contador += 1

hilos = [threading.Thread(target=incrementar) for _ in range(5)]
for h in hilos: h.start()
for h in hilos: h.join()
print(f"Contador con lock: {contador} (esperado: 5000)")

# 2. Semaphore
print("\n=== 2. Semaphore (max 2 accesos simultaneos) ===")
semaforo = threading.Semaphore(2)

def acceder(id):
    with semaforo:
        print(f"Hilo {id} accedio al recurso")
        time.sleep(0.2)

hilos = [threading.Thread(target=acceder, args=(i,)) for i in range(5)]
for h in hilos: h.start()
for h in hilos: h.join()

# 3. Monitor / Condition
print("\n=== 3. Monitor / Condition ===")
cola = deque()
cond = threading.Condition()

def productor():
    for i in range(1, 6):
        with cond:
            cola.append(i)
            print(f"Producido: {i}")
            cond.notify()
        time.sleep(0.1)

def consumidor():
    for _ in range(5):
        with cond:
            while not cola:
                cond.wait()
            print(f"Consumido: {cola.popleft()}")
        time.sleep(0.2)

p = threading.Thread(target=productor)
c = threading.Thread(target=consumidor)
p.start()
c.start()
p.join()
c.join()

# 4. Atomic operations (simulado con Lock)
print("\n=== 4. Atomic operations ===")
contador_atomico = 0
lock_atomico = threading.Lock()

def incrementar_atomico():
    global contador_atomico
    for _ in range(1000):
        with lock_atomico:
            contador_atomico += 1

hilos = [threading.Thread(target=incrementar_atomico) for _ in range(5)]
for h in hilos: h.start()
for h in hilos: h.join()
print(f"Contador atomico: {contador_atomico} (esperado: 5000)")

# 5. Critical Section
print("\n=== 5. Critical Section ===")
print("Seccion critica: codigo dentro de with lock:")
print("Solo un hilo ejecuta esa seccion a la vez.")
