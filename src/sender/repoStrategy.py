import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket

from abc import ABC, abstractmethod

@abstractmethod
class RepoStrategy(ABC):
    @abstractmethod
    def __init__(self, repository_) -> None:
        self.terminate = False
        self.repository = repository_

    @abstractmethod
    def send(self):
        pass

    @abstractmethod
    def end(self):
        pass

class RepoStrategyFactory():
    def get_strategy(strategy_name, repository_):
        if strategy_name == 'udp':
            return UDPRepo(repository_)
        elif strategy_name == 'firebase':
            return FirebaseRepo(repository_)
        else:
            raise Exception('Invalid strategy name')
        
class UDPRepo(RepoStrategy):
    def __init__(self, repository_) -> None:
        super().__init__(repository_)
        self.listen_ip = '0.0.0.0'
        self.listen_port = 5004
        self.send_ip = '127.0.0.1'
        self.send_port = 5005

    def end(self):
        self.terminate = True

    def listen_for_data_thread(self):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        _socket.bind((self.listen_ip, self.listen_port))
        
        try:
            while not self.terminate:
                data, addr = _socket.recvfrom(1024)
                message = data.decode('utf-8')
                if message == 'STOP':
                    print("Exiting...")
                    self.terminate = True
                    break
        
        except Exception as e:
            print(e)
        finally:
            _socket.close()
            self.terminate = True

    def send_specified_data(self, data):
        self.data = data
        self.send()

    def send(self):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            message = str(self.repository.data)
            message_bytes = message.encode('utf-8')
            _socket.sendto(message_bytes, (self.send_ip, self.send_port))
            _socket.close()
        except Exception as e:
            print(e)
    
class FirebaseRepo(RepoStrategy):
    def __init__(self, repository_):
        super().__init__(repository_)
        self.credentials = credentials.Certificate("./key/firebase_table-key.json")
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        })

    def setup(self):
        self.send_data_thread = threading.Thread(target=self.send_data_thread)
        self.send_data_thread.daemon = True
        self.send_data_thread.start()
        
    def send_data_thread(self):
        while not self.terminate:
            if self.new_data:
                self.send()
                self.new_data = False

    def send(self):
        try:
            ref = db.reference('/')
            ref.set([self.data])
        except Exception as e:
            print("Error sending data:", e)

    def end(self):
        self.terminate = True
        if self.send_data_thread:
            self.send_data_thread.join()
        firebase_admin.delete_app(self.firebase_admin)