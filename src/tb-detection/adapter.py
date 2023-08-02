import os
import json

class Exporter:
    def __init__(self, destination):
        self.destination = destination
        self.json = {}

    def setup(self):
        if not os.path.exists(self.destination):
            os.makedirs(self.destination)
    
    def export(self, json_data):
        # Serializing json
        json_object = json.dumps(str(json_data))
        
        # Writing to sample.json
        with open(self.destination + "points.json", "w") as outfile:
            outfile.write(json_object)