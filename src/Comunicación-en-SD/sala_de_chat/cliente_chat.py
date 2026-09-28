import socket
import threading
import sys

# * Dirección IP y Puerto del servidor de chat
# ! Se puede pasar la IP de la otra computadora como argumento (ej: python3 cliente_chat.py 192.168.1.50)
IP_DESTINO = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_DESTINO = 7000

def recibir_mensajes(sock):
    """
    * Hilo secundario: Escucha constantemente los mensajes que envía el servidor
    * (tanto los mensajes de otros usuarios como las notificaciones del sistema).
    """
    while True:
        try:
            datos = sock.recv(1024)
            # ! Si recv devuelve vacío, el servidor se apagó o cerró la conexión
            if not datos:
                print("\n[!] Conexión perdida con el servidor de chat.")
                break
            # * Imprimir mensaje recibido de otro participante
            print(datos.decode('utf-8'), end="")
        except Exception:
            # ! El socket se cerró al salir del programa
            break

def iniciar_cliente():
    # * 1. Crear socket TCP
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    print(f"[*] Conectando al servidor de chat en {IP_DESTINO}:{PUERTO_DESTINO}...")
    try:
        sock.connect((IP_DESTINO, PUERTO_DESTINO))
    except ConnectionRefusedError:
        print("[!] Error: No se pudo conectar. ¿El servidor_chat.py está encendido en esa IP?")
        return

    # * 2. Pedir apodo/nombre de usuario
    apodo = input("👉 Ingresá tu nombre o apodo para el chat: ").strip()
    while not apodo:
        apodo = input("👉 Por favor, ingresá un nombre válido: ").strip()
        
    # * Enviar el apodo al servidor como primer mensaje
    sock.sendall(apodo.encode('utf-8'))

    # * 3. Iniciar hilo en segundo plano para RECIBIR mensajes de los demás
    # ! Usamos un Thread para que no se bloquee la pantalla mientras escribimos
    hilo_receptor = threading.Thread(target=recibir_mensajes, args=(sock,))
    hilo_receptor.daemon = True
    hilo_receptor.start()

    print("\n✅ ¡Conectado con éxito! Ya podés chatear.")
    print("Escribí tus mensajes y presioná Enter. Escribí 'salir' para desconectarte.\n")

    # * 4. Bucle principal: ENVIAR mensajes escritos por el usuario
    try:
        while True:
            mensaje = input()
            if not mensaje.strip():
                continue
                
            if mensaje.lower().strip() == 'salir':
                sock.sendall("salir".encode('utf-8'))
                break
                
            # * Enviar el mensaje al servidor para que lo distribuya (Broadcast)
            sock.sendall(mensaje.encode('utf-8'))

    except KeyboardInterrupt:
        print("\n[*] Saliendo del chat...")
    finally:
        # ! Cerrar conexión limpiamente
        sock.close()
        print("[*] Desconectado de la sala.")

if __name__ == "__main__":
    iniciar_cliente()
