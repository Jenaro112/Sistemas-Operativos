import socket
import sys

# * La IP del servidor se puede pasar como argumento por consola.
# ! Si no se pasa ninguna, asume que están en la misma computadora (127.0.0.1)
IP_DESTINO = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_DESTINO = 5000

# * 1. Crear socket con interfaz de datagramas (UDP)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print(f"[*] Cliente UDP iniciado.")
print(f"[*] Configurado para enviar mensajes a la IP: {IP_DESTINO} al puerto {PUERTO_DESTINO}")
print("[*] Escribí tu mensaje y presioná Enter. Escribí 'salir' para terminar.\n")

try:
    while True:
        # ? Leer mensaje del usuario desde la terminal
        mensaje = input("Tu mensaje: ")
        
        if mensaje.strip().lower() == 'salir':
            break
        
        # * 2. Enviar el mensaje
        # ! Usamos sendto() especificando a dónde queremos enviar el datagrama
        # * encode('utf-8') convierte el texto a bytes
        sock.sendto(mensaje.encode('utf-8'), (IP_DESTINO, PUERTO_DESTINO))
        
except KeyboardInterrupt:
    print("\n[*] Interrumpido por el usuario.")
finally:
    # ! 3. Cerrar el socket al terminar
    print("[*] Apagando cliente...")
    sock.close()
