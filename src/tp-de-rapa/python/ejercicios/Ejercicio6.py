import threading
import time

recurso = threading.Lock()
accesos = {}

def hilo_rapido(id):
    for i in range(20):
        with recurso:
            accesos[id] = accesos.get(id, 0) + 1
            print(f"Hilo rapido {id} accedio (total: {accesos[id]})")

def hilo_lento():
    for i in range(5):
        with recurso:
            print(f">>> Hilo LENTO accedio (vez {i + 1})")
        time.sleep(0.05)

# Hilos rapidos
for i in range(1, 4):
    t = threading.Thread(target=hilo_rapido, args=(i,))
    t.daemon = True
    t.start()

# Hilo lento
lento = threading.Thread(target=hilo_lento)
lento.start()
lento.join()

print("\nLos hilos rapidos acaparan el recurso y el lento")
print("apenas logra acceder (starvation).")

