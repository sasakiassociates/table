import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket
import threading

class Repository():
    def __init__(self, url, key_path, ip, port, mode):
        # self.credentials = credentials.Certificate("../key/magpietable-firebase-adminsdk-2sqk3-9ecfe00124.json")
        # self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
        #     'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        # })
        self.credentials = credentials.Certificate(key_path)
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': url
        })
        self.data = {}
        self.mode = mode
        self.ip = ip
        self.port = port

        self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.udp_socket.bind((self.ip, self.port))

    def add_to_data(self, marker_id_, data):
        self.data.setdefault(str(marker_id_), data).update()

    def send_data(self):
        if self.mode == "u":
            self.udp_send_data()
        elif self.mode == "f":
            self.send_data_firebase()
        elif self.mode == "b":
            self.send_data_firebase()
            self.udp_send_data()
        else:
            print("Error: mode not recognized")
        self.data = {}

    def udp_start_listening(self):
        def udp_listen_thread_func():
            while True:
                self.udp_listen_event.wait()
                self.udp_listen_event.clear()
                self.udp_send_data()
        
        if not self.udp_listen_thread:
            self.udp_listen_thread = threading.Thread(target=udp_listen_thread_func)
            self.udp_listen_thread.daemon = True
            self.udp_listen_thread.start()

    def udp_listen_for_command(self):
        self.udp_socket.bind((self.ip, self.port))
        while True:
            command, _ = self.udp_socket.recvfrom(1024)
            print("Received message:", command.decode('utf-8'))
            if command == b'Send data!':
                self.udp_listen_event.set()

    def udp_send_data(self):
        try:
            message = str([self.data])
            message_bytes = message.encode('utf-8')
            self.udp_socket.sendto(message_bytes, (self.ip, self.port))
        except Exception as e:
            print("Error sending data:", e)
    
    def send_data_firebase(self):
        try:
            ref = db.reference('/')
            ref.set([self.data])
        except Exception as e:
            print("Error sending data:", e)
    
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)