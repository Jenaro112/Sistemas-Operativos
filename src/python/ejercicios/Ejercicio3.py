import threading
import time
from collections import deque

CAPACIDAD = 5
cola = deque()
lock = threading.Lock()
no_llena = threading.Condition(lock)
no_vacia = threading.Condition(lock)

def productor():
    for i in range(1, 21):
        with no_llena:
            while len(cola) == CAPACIDAD:
                no_llena.wait()
            cola.append(i)
            print(f"Producido: {i}")
            no_vacia.notify()
        time.sleep(0.3)

def consumidor():
    for _ in range(20):
        with no_vacia:
            while not cola:
                no_vacia.wait()
            valor = cola.popleft()
            print(f"Consumido: {valor}")
            no_llena.notify()
        time.sleep(0.5)

hilo1 = threading.Thread(target=productor)
hilo2 = threading.Thread(target=consumidor)

hilo1.start()
hilo2.start()

hilo1.join()
hilo2.join()

"""
Preguntas:
1. ¿Por qué se necesita sincronización?
   Para evitar condiciones de carrera. Sin sincronización, productor
   y consumidor podrían acceder/modificar la cola al mismo tiempo,
   corrompiendo los datos (inconsistencia).

2. ¿Qué error aparece si productor y consumidor acceden libremente a la cola?
   Data race / condicion de carrera. Pueden perderse elementos,
   duplicarse, o la cola puede quedar en un estado inconsistente
   (ej. un popleft() sobre una cola vacia lanza IndexError).

3. ¿Qué ventajas tiene usar una cola thread-safe?
   - Elimina la necesidad de gestionar locks manualmente.
   - Reduce errores como deadlocks o condiciones de carrera.
   - El codigo es mas limpio y legible.
"""
