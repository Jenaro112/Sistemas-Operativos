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

