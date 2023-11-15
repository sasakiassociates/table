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
    
    def remove(self, object_type, uuid):
        if object_type in self.data and str(uuid) in self.data[object_type]:
            del self.data[str(object_type)][str(uuid)]
            if self.data[str(object_type)] == {}:
                del self.data[str(object_type)]
        self.new_data = True

    def update(self, uuid, json, object_type):
        if object_type not in self.data:
            self.data[str(object_type)] = {}
        self.data[str(object_type)][str(uuid)] = json
        self.new_data = True
        
    # def update(self, marker_json):
    #     if marker_json['location'] == [0, 0, 0]:
    #         self.data.pop(str(marker_json['id']), None)
    #     else:
    #         self.data[str(marker_json['id'])] = marker_json
    #         # self.data.setdefault(str(marker_json['id']), marker_json).update()
    #     self.new_data = True
        
if (__name__ == '__main__'):
    print("Running unit tests for repository.py")
    repo = Repository('udp')
    repo.update({'id': 0, 'location': [10, -200], 'rotation': 0, 'type': 'geometry', 'name': 'Geometry 0'}, 1)
    repo.update({'id': 1, 'location': [20, 200], 'rotation': 0.5, 'type': 'geometry', 'name': 'Geometry 1'}, 2)
    repo.update({'id': 2, 'location': [30, 500], 'rotation': 0.5, 'type': 'geometry'}, 3)
