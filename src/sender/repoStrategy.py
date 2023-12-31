import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket

from abc import ABC, abstractmethod

@abstractmethod
class RepoStrategy(ABC):
    @abstractmethod
    def __init__(self, project_name, event_manager) -> None:
        self.terminate = False
        self.event_manager = event_manager

    @abstractmethod
    def push(self, message):
        pass

class RepoStrategyFactory():
    def get_strategy(strategy_name, project_name, event_manager):
        if strategy_name == 'udp':
            return UDPRepo(project_name, event_manager)
        elif strategy_name == 'firebase':
            return FirebaseRepo(project_name, event_manager)
        elif strategy_name == 'both':
            composite_repo = CompositeRepo(project_name)
            composite_repo.add_strategy(UDPRepo(project_name, event_manager))
            composite_repo.add_strategy(FirebaseRepo(project_name, event_manager))
            return composite_repo
        else:
            raise Exception('Invalid strategy name')
        
class UDPRepo(RepoStrategy):
    def __init__(self, project_name, event_manager) -> None:
        super().__init__(project_name, event_manager)
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

    def push(self, message):
        _socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            message = str(message)
            # print(message)
            message_bytes = message.encode('utf-8')
            _socket.sendto(message_bytes, (self.send_ip, self.send_port))
            _socket.close()
        except Exception as e:
            print(e)

class FirebaseRepo(RepoStrategy):
    def __init__(self, project_name, event_manager):
        super().__init__(project_name, event_manager)
        self.credentials = credentials.Certificate("./sender/key/firebase_table-key.json")
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        })
        self.marker_ref = db.reference(f'/bases/{project_name}/marker')
        self.config_ref = db.reference(f'/bases/{project_name}/config')
        # Begins the listening thread
        # self.listener = self.config_ref.listen(self.on_new_data)
        self.project_name = None

        # Clear the markers from the previous run
        # self.marker_ref.delete()

    # Does this need a reference to the event manager?
    def on_new_data(self, event):
        if event.data is None:
            return
        # config data includes:
        # - registration points
        # - marker update interval
        # - base marker id
        self.event_manager.register_event({"type": "config_update", "data": event.data})

    def remove_marker(self, marker_id):
        self.marker_ref.child(marker_id).delete()

    def end(self):
        self.terminate = True
        # Clear any leftover markers from this run
        self.marker_ref.delete()
        firebase_admin.delete_app(self.firebase_admin)

    def push(self, message):
        try:
            self.marker_ref.update(message)
        except Exception as e:
            print("Error sending data:", e)

class CompositeRepo(RepoStrategy):
    def __init__(self, project_name):
        super().__init__(project_name)
        self.strategies = []
    
    def add_strategy(self, strategy):
        self.strategies.append(strategy)
    
    def send(self, message):
        for strategy in self.strategies:
            strategy.send(message)
    
    def end(self):
        for strategy in self.strategies:
            strategy.end()