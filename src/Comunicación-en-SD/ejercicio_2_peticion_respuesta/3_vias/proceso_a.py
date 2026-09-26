import socket
import sys
import time

# ! IP y Puertos cruzados según consigna
IP_B = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Inicialización del Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", PUERTO_A_RECV))

print("--- PROTOCOLO 3 VÍAS (Proceso A - Petición) ---")

# * 1. Enviar REQUEST
print(f"[1] Enviando REQUEST a B...")
sock.sendto(b"REQUEST:DatoX", (IP_B, PUERTO_B_RECV))

# * 2. Recibir REPLY
# ! Espera activa hasta recibir el resultado
datos, _ = sock.recvfrom(1024)
print(f"[2] Recibido REPLY de B: {datos.decode()}")

# * 3. Enviar ACK de la Respuesta
time.sleep(1)
# ! Se agrega la 3ra vía: A confirma a B que la respuesta fue entregada con éxito
print(f"[3] Enviando ACK a B (Confirmando que me llegó la respuesta)...")
sock.sendto(b"ACK:Recibi_tu_REPLY", (IP_B, PUERTO_B_RECV))

# ! Cierre
sock.close()
print("Proceso terminado.")
