import repoStrategy as rs

class Repository():
    def __init__(self, strategy_name):
        self.data = {}
        self.strategy = rs.RepoStrategyFactory.get_strategy(strategy_name)

    def add_to_data(self, marker_id, data):
        self.data.setdefault(str(marker_id), data).update()

    def setup(self):
        self.strategy.setup()

    # Update the UDP thread with the data
    def send_data(self):
        self.strategy.set_data(self.data)
        self.data = {}

    def check_for_terminate(self):
        return self.strategy.terminate
        
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)