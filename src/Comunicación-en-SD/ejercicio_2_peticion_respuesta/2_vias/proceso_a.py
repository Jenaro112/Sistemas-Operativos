import socket
import sys

# ! IP de destino (Por defecto localhost si no se especifica en consola)
IP_B = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
# ! Puertos cruzados según consigna
PUERTO_A_RECV = 5001  # * Puerto donde Proceso A escucha
PUERTO_B_RECV = 5002  # * Puerto donde Proceso B escucha

# * Inicialización del socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", PUERTO_A_RECV))

print("--- PROTOCOLO 2 VÍAS (Proceso A - Petición) ---")
print(f"Escuchando respuestas en el puerto {PUERTO_A_RECV}")

# * 1. Enviar REQUEST
print(f"[1] Enviando REQUEST a Proceso B ({IP_B}:{PUERTO_B_RECV})...")
sock.sendto(b"REQUEST:Necesito_Informacion", (IP_B, PUERTO_B_RECV))

# * 2. Recibir REPLY
# ! recvfrom es bloqueante, el proceso espera hasta recibir la respuesta
datos, _ = sock.recvfrom(1024)
print(f"[2] Recibido REPLY de B: {datos.decode()}")

# ! Cierre del socket
sock.close()
print("Proceso terminado.")
