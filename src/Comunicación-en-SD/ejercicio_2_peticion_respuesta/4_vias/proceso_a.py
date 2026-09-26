import socket
import sys
import time

# ! IPs y Puertos cruzados
IP_B = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# ! PREVENCIÓN ERROR 48
sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
sock.bind(("0.0.0.0", PUERTO_A_RECV))

print("--- PROTOCOLO 4 VÍAS (Proceso A - Petición) ---")

# * 1. Enviar REQUEST
print(f"[1] Enviando REQUEST a B...")
sock.sendto(b"REQUEST:Procesar_Info", (IP_B, PUERTO_B_RECV))

# * 2. Recibir ACK del Request
# ! B avisa que le llegó la petición (importante para que A no retransmita)
datos_ack_req, _ = sock.recvfrom(1024)
print(f"[2] Recibido ACK de B: {datos_ack_req.decode()}")

# * 3. Recibir REPLY (La respuesta real)
# ! Ahora A espera el resultado del procesamiento
datos_reply, _ = sock.recvfrom(1024)
print(f"[3] Recibido REPLY de B: {datos_reply.decode()}")

# * 4. Enviar ACK del Reply
time.sleep(1)
# ! A notifica que recibió el resultado con éxito
print(f"[4] Enviando ACK final a B...")
sock.sendto(b"ACK:Recibi_REPLY_Final", (IP_B, PUERTO_B_RECV))

# ! Limpieza y fin
sock.close()
print("Proceso terminado.")
