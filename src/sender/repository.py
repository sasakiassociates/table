# import repoStrategy as rs
from . import repoStrategy as rs
import threading

class Repository():
    def __init__(self, strategy_name):
        self.data = {}
        self.new_data = False
        self.strategy = rs.RepoStrategyFactory.get_strategy(strategy_name, self)

    def close_threads(self):
        if self.strategy.terminate == False:
            self.strategy.terminate = True

    def check_for_terminate(self):
        return self.strategy.terminate
    
    def remove_from_sent_data(self, uuid):
        uuid = str(uuid)
        if uuid in self.data:
            del self.data[uuid]
            self.new_data = True

    def update(self, uuid, json):
        uuid = str(uuid)
        if "marker" not in self.data: self.data["marker"]    = {}
        self.data["marker"][uuid] = json
        
        self.new_data = True
        
if (__name__ == '__main__'):
    print("Running unit tests for repository.py")
    repo = Repository('udp')
    repo.update({'id': 0, 'location': [10, -200], 'rotation': 0, 'type': 'geometry', 'name': 'Geometry 0'}, 1)
    repo.update({'id': 1, 'location': [20, 200], 'rotation': 0.5, 'type': 'geometry', 'name': 'Geometry 1'}, 2)
    repo.update({'id': 2, 'location': [30, 500], 'rotation': 0.5, 'type': 'geometry'}, 3)
