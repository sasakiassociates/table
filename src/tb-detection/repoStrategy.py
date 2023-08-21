import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket
import threading

from abc import ABC, abstractmethod

class RepoStrategy(ABC):
    @abstractmethod
    def __init__(self) -> None:
        self.data = {}
        self.terminate = False

    @abstractmethod
    def setup():
        pass

    @abstractmethod
    def send_data():
        pass

class RepoStrategyFactory():
    def get_strategy(strategy_name):
        if strategy_name == 'udp':
            return UDPRepo()
        elif strategy_name == 'http':
            return HTTPRepo()
        else:
            raise Exception('Invalid strategy name')
        
class UDPRepo(RepoStrategy):
    def __init__(self) -> None:
        super().__init__()
        self.listen_ip = '0.0.0.0'
        self.listen_port = 5004
        self.send_ip = '127.0.0.1'
        self.send_port = 5005

    def setup(self):
        # create and run thread to listen for commands
        listen_thread = threading.Thread(target=self.listen_for_data_thread)
        listen_thread.daemon = True
        listen_thread.start()

    def listen_for_data_thread(self):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        _socket.bind((self.listen_ip, self.listen_port))

        try:
            while True:
                data, addr = _socket.recvfrom(1024)
                message = data.decode('utf-8')
                if message == 'SEND':
                    print("Sending data..." + str(self.data))
                    self.send_data()
                elif message == 'END':
                    print("Exiting...")
                    break
        except Exception as e:
            print(e)
        finally:
            _socket.close()
            self.terminate = True

    def send_data(self):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            message = str(self.data)
            message_bytes = message.encode('utf-8')
            _socket.sendto(message_bytes, (self.send_ip, self.send_port))
            _socket.close()
        except Exception as e:
            print(e)
    
    def set_data(self, data):
        self.data = data
    
class HTTPRepo(RepoStrategy):
    # self.credentials = credentials.Certificate("../key/magpietable-firebase-adminsdk-2sqk3-9ecfe00124.json")
        # self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
        #     'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        # })
        # self.credentials = credentials.Certificate(key_path)
        # self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
        #     'databaseURL': url
        # })
    def __init__(self):
        super().__init__()
        self.credentials = credentials.Certificate("../key/firebase_table-key.json")
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        })

    def send_data(self):
        try:
            ref = db.reference('/')
            ref.set([self.data])
        except Exception as e:
            print("Error sending data:", e)