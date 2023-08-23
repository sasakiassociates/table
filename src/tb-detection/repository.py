class Repository():
    def __init__(self, strategy):
        self.data = {}
        self.strategy = strategy
        self.model_num = 0
        self.variable_num = 0

    def add_to_data(self, marker_id_, data):
        self.data.setdefault(str(marker_id_), data).update()

    def setup(self):
        self.strategy.setup()

    def get_setup_data(self):
        self.model_num = self.strategy.model_num
        self.variable_num = self.strategy.variable_num

    # Update the UDP thread with the data
    def update_send_data(self):
        self.strategy.set_data(self.data)
        self.data = {}

    def check_for_terminate(self):
        return self.strategy.terminate
    
    def check_for_launch(self):
        return self.strategy.launch
    
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)