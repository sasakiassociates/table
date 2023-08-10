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
        self.calculate_rotation(corners_)
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
    
    # TODO fix this; currently is only saying the rotation is around 130 degrees
    def calculate_rotation(self, corners_):
        corner1 = corners_[0][0]
        corner2 = corners_[0][1]
        corner3 = corners_[0][2]
        corner4 = corners_[0][3]
        
        # gets vector in one direction
        vector1 = (corner1[0] - corner2[0], corner1[1] - corner2[1])
        # gets vector in other direction
        vector2 = (corner3[0] - corner1[0], corner3[1] - corner1[1])

        dot_product = vector1[0] * vector2[0] + vector1[1] * vector2[1]

        magnitude1 = np.sqrt(vector1[0] ** 2 + vector1[1] ** 2)
        magnitude2 = np.sqrt(vector2[0] ** 2 + vector2[1] ** 2)
        
        cosine_angle = dot_product / (magnitude1 * magnitude2)

        angle_radians = np.arccos(cosine_angle)
        # return self.rotation

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