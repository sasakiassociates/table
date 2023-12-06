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
    
    def remove_from_sent_data(self, id_, uuid):
        uuid = str(uuid)
        if id_ in self.data:
            if uuid in self.data[id_]:
                del self.data[id_][uuid]
                if self.data[id_] == {}:
                    del self.data[id_]
                self.new_data = True

    def update(self, id_, uuid, json):
        uuid = str(uuid)
        if id_ not in self.data:
            self.data[id_] = {}
        self.data[id_][uuid] = json
        
        self.new_data = True
        
# {2: 
#   {
#   '7602833e-d34f-4f82-a22f-fdc66a3be1f1': 
#       {'id': 2, 'uuid': '7602833e-d34f-4f82-a22f-fdc66a3be1f1', 'location': [-800, -124, 0], 'rotation': 2.1096158842252324}, 
#   '7f4f217b-cba2-4687-a428-997274c3cd14': 
#       {'id': 2, 'uuid': '7f4f217b-cba2-4687-a428-997274c3cd14', 'location': [-791, -166, 0], 'rotation': 2.1269625047849248}
#   }
# }
        
if (__name__ == '__main__'):
    print("Running unit tests for repository.py")
    repo = Repository('udp')
    repo.update({'id': 0, 'location': [10, -200], 'rotation': 0, 'type': 'geometry', 'name': 'Geometry 0'}, 1)
    repo.update({'id': 1, 'location': [20, 200], 'rotation': 0.5, 'type': 'geometry', 'name': 'Geometry 1'}, 2)
    repo.update({'id': 2, 'location': [30, 500], 'rotation': 0.5, 'type': 'geometry'}, 3)
