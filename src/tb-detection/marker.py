import numpy as np
from abc import ABC, abstractmethod

#TODO get rid of isVisible parameter
class Marker(ABC):
    def __init__(self, marker_id_):
        self.id = marker_id_
        self.observers = []

    def attach_observer(self, observer_):
        self.observers.append(observer_)
    
    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.build_json(), self.id)
    
    def update(self, corners_):
        self.calculate_center(corners_)
        self.notify_observers()
    
    def get_id(self):
        return self.id
    
    def build_json(self):
        marker_data = {
            "id": self.id,
            "location": [self.center[0], self.center[1]],
            "rotation": 0,
        }
        return marker_data

class ModelMarker(Marker):
    def __init__(self, marker_id_):
        super().__init__(marker_id_)
        self.center = None

    def calculate_center(self, corners_):
        np_center = np.mean(corners_[0], axis=0)
        self.center = (int(np_center[0]), int(np_center[1]))
    
    def get_center(self):
        return self.center
    
class ModeMarker(Marker):
    def __init__(self, marker_id_):
        super().__init__(marker_id_)

    def calculate_rotation(self, corners_):
        pass