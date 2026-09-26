import socket
import sys

# * Configuración del servidor destino
IP_DESTINO = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_DESTINO = 6000

# * 1. Crear socket TCP
# ! SOCK_STREAM = TCP (Provee orden garantizado y entrega confiable, evitando protocolos de N-vías manuales)
sock_cliente = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

print(f"[*] Intentando conectar a {IP_DESTINO}:{PUERTO_DESTINO}...")

try:
    # * 2. Conectar al servidor (Establishment)
    # ! Este paso es obligatorio en TCP. Falla si el servidor no está encendido.
    sock_cliente.connect((IP_DESTINO, PUERTO_DESTINO))
    
    print("[+] ¡Conectado al servidor TCP de forma confiable!")
    print("[*] Escribí un mensaje y presioná Enter. Escribí 'salir' para terminar.\n")

    while True:
        # ? Leer entrada del usuario
        mensaje = input("Mensaje TCP: ")
        
        if mensaje.lower().strip() == 'salir':
            break
        
        # * 3. Enviar datos (Garantizados de llegar intactos por la capa de transporte)
        sock_cliente.sendall(mensaje.encode('utf-8'))

        # * 4. Esperar la respuesta del servidor (Bloqueante)
        datos_recibidos = sock_cliente.recv(1024)
        print(f"[Servidor TCP] -> {datos_recibidos.decode('utf-8')}")

# ! Manejo de errores de conexión comunes
except ConnectionRefusedError:
    print("[!] Error: No se pudo conectar. ¿Asegurate de que el servidor esté encendido?")
except KeyboardInterrupt:
    print("\n[*] Desconectando a petición del usuario...")
finally:
    # * 5. Cerrar la conexión limpiamente (Envía flag FIN)
    sock_cliente.close()
    print("[*] Conexión TCP cerrada de forma segura.")
