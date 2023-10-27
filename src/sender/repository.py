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
            self.strategy.teminate = True

    def check_for_terminate(self):
        return self.strategy.terminate
        
    def update(self, uuid, marker_json):
        if marker_json['location'] == [0, 0, 0]:
            self.data.pop(str(uuid), None)
        else:
            self.data[str(uuid)] = marker_json
            # self.data.setdefault(str(marker_json['id']), marker_json).update()
        self.new_data = True
        
if (__name__ == '__main__'):
    print("Running unit tests for repository.py")
    repo = Repository('udp')
    repo.update({'id': 0, 'location': [10, -200], 'rotation': 0, 'type': 'geometry', 'name': 'Geometry 0'}, 1)
    repo.update({'id': 1, 'location': [20, 200], 'rotation': 0.5, 'type': 'geometry', 'name': 'Geometry 1'}, 2)
    repo.update({'id': 2, 'location': [30, 500], 'rotation': 0.5, 'type': 'geometry'}, 3)
