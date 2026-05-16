import threading
import time

recurso_a = threading.Lock()
recurso_b = threading.Lock()

print("=== DEADLOCK ===")

def hilo1_deadlock():
    with recurso_a:
        print("Hilo 1: bloqueo RecursoA")
        time.sleep(0.1)
        with recurso_b:
            print("Hilo 1: bloqueo RecursoB")

def hilo2_deadlock():
    with recurso_b:
        print("Hilo 2: bloqueo RecursoB")
        time.sleep(0.1)
        with recurso_a:
            print("Hilo 2: bloqueo RecursoA")

h1 = threading.Thread(target=hilo1_deadlock)
h2 = threading.Thread(target=hilo2_deadlock)
h1.start()
h2.start()
h1.join(timeout=2)
h2.join(timeout=2)
print("(Si no termina, es deadlock)")

print("\n=== CORREGIDO (mismo orden) ===")

def hilo1_corregido():
    with recurso_a:
        print("Hilo 1: bloqueo RecursoA")
        time.sleep(0.1)
        with recurso_b:
            print("Hilo 1: bloqueo RecursoB")

def hilo2_corregido():
    with recurso_a:
        print("Hilo 2: bloqueo RecursoA")
        time.sleep(0.1)
        with recurso_b:
            print("Hilo 2: bloqueo RecursoB")

h1 = threading.Thread(target=hilo1_corregido)
h2 = threading.Thread(target=hilo2_corregido)
h1.start()
h2.start()
h1.join()
h2.join()

"""
Preguntas:
1. ¿Por qué se produce el deadlock?
   Porque cada hilo tiene un recurso que el otro necesita y ninguno
   libera el suyo. Quedan esperandose mutuamente para siempre.

2. ¿Qué condiciones deben cumplirse para que exista deadlock?
   Las 4 condiciones de Coffman:
   - Exclusion mutua: los recursos no son compartibles.
   - Retencion y espera: un hilo retiene un recurso mientras espera otro.
   - No desalojo: un recurso no puede ser quitado forzosamente.
   - Espera circular: existe un ciclo de hilos esperando recursos.

3. ¿Cómo se puede prevenir?
   - Bloquear recursos siempre en el mismo orden (rompe espera circular).
   - Usar timeout con tryLock (rompe retencion y espera).
   - Minimizar el tiempo dentro de secciones criticas.
"""
