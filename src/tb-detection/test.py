import socket

UDP_PORT = 5005
UDP_IP = "127.0.0.1"

def test_udp_receive():
    return

def test_udp_send():
    message = "Hello C# world from Python world"
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        message_bytes = message.encode('utf-8')
        udp_socket.sendto(message_bytes, (UDP_IP, UDP_PORT))
        udp_socket.close()
    except Exception as e:
        print("Error sending data:", e)

test_udp_send()