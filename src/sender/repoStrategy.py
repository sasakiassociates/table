import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket

from abc import ABC, abstractmethod

@abstractmethod
class RepoStrategy(ABC):
    @abstractmethod
    def __init__(self, repository_, project_name) -> None:
        self.terminate = False

    @abstractmethod
    def send(self, message):
        pass

    @abstractmethod
    def end(self):
        pass

class RepoStrategyFactory():
    def get_strategy(strategy_name, repository_, project_name):
        if strategy_name == 'udp':
            return UDPRepo(repository_, project_name)
        elif strategy_name == 'firebase':
            return FirebaseRepo(repository_, project_name)
        elif strategy_name == 'both':
            composite_repo = CompositeRepo(repository_, project_name)
            composite_repo.add_strategy(UDPRepo(repository_, project_name))
            composite_repo.add_strategy(FirebaseRepo(repository_, project_name))
            return composite_repo
        else:
            raise Exception('Invalid strategy name')
        
class UDPRepo(RepoStrategy):
    def __init__(self, repository_, project_name) -> None:
        super().__init__(repository_, project_name)
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

    def send(self, message):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            message = str(message)
            # print(message)
            message_bytes = message.encode('utf-8')
            _socket.sendto(message_bytes, (self.send_ip, self.send_port))
            _socket.close()
        except Exception as e:
            print(e)
    
# TODO need to wipe the markers that are there on startup
class FirebaseRepo(RepoStrategy):
    def __init__(self, repository_, project_name):
        super().__init__(repository_, project_name)
        self.credentials = credentials.Certificate("./sender/key/firebase_table-key.json")
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        })
        self.ref = db.reference(f'/bases/{project_name}/marker')

    def listen_for_data_thread(self):
        self.ref.listen(self.listener)

    # TODO This should be changed so that each marker sends it's own json directly to firebase and doesn't concatenate them
    def send(self, message):
        try:
            self.ref.update(message)
            # ref.set(message)
        except Exception as e:
            print("Error sending data:", e)

    def remove(self, uuid):
        self.ref.child(uuid).delete()

    def end(self):
        self.terminate = True
        if self.send_data_thread:
            self.send_data_thread.join()
        firebase_admin.delete_app(self.firebase_admin)

class CompositeRepo(RepoStrategy):
    def __init__(self, repository_):
        super().__init__(repository_)
        self.strategies = []
    
    def add_strategy(self, strategy):
        self.strategies.append(strategy)
    
    def send(self, message):
        for strategy in self.strategies:
            strategy.send(message)
    
    def end(self):
        for strategy in self.strategies:
            strategy.end()