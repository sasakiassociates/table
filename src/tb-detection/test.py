import socket

UDP_SEND_PORT = 5005
UDP_LISTEN_PORT = 5004

UDP_IP = "127.0.0.1"
UDP_LISTEN_IP = "0.0.0.0"

def test_udp_receive():
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_socket.bind((UDP_LISTEN_IP, UDP_LISTEN_PORT))

    try:
        while True:
            data, addr = udp_socket.recvfrom(1024)
            print("received message:", data.decode('utf-8'))
            if data == b'SEND':
                # This is where the marker data will send to the client
                print("Sending message back to client...")
                test_udp_send("Hello!")
            # This ends the whole python program
            elif data == b'END':
                print("Exiting...")
                break

    except KeyboardInterrupt:
        print("Keyboard interrupt, exiting...")
    finally:
        udp_socket.close()
        udp_socket

def test_udp_send(message):
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        message_bytes = message.encode('utf-8')
        udp_socket.sendto(message_bytes, (UDP_IP, UDP_SEND_PORT))
        udp_socket.close()
    except KeyboardInterrupt:
        print("Keyboard interrupt, exiting...")
    finally:
        udp_socket.close()

# test_udp_send()
test_udp_receive()