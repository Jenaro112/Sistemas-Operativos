import socket
import sys
import time

# ! Configuración de IPs y Puertos cruzados
IP_A = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 3 VÍAS (Proceso B - Respuesta) ---")

# * 1. Recibir REQUEST
datos, _ = sock.recvfrom(1024)
print(f"[1] Recibido REQUEST de A: {datos.decode()}")

# * 2. Enviar REPLY
time.sleep(1)
print(f"[2] Enviando REPLY a A...")
sock.sendto(b"REPLY:Aqui_tienes_DatoX", (IP_A, PUERTO_A_RECV))

# * 3. Recibir ACK de la Respuesta
# ! En vez de cerrar de inmediato, B se bloquea esperando la confirmación de llegada
datos_ack, _ = sock.recvfrom(1024)
print(f"[3] Recibido ACK de A: {datos_ack.decode()}")

# ! Cierre final
sock.close()
print("Proceso terminado.")
