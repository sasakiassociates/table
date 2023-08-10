import numpy as np
from abc import ABC, abstractmethod

#TODO get rid of isVisible parameter
class Marker(ABC):
    def __init__(self, marker_id_):
        self.id = marker_id_
        self.observers = []
        self.rotation = 0

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
            "rotation": self.rotation,
        }
        return marker_data
    
    # TODO fix this; currently is only saying the rotation is around 130 degrees
    def calculate_rotation(self, corners_):
        corners = corners_.reshape(-1,2)

        centroid = np.mean(corners, axis=0)

        reference_vector = corners[0] - centroid

        angle_rads = np.arctan2(reference_vector[1], reference_vector[0])
        angle_degree = np.degrees(angle_rads)
        wrapped_angle = self.wrap_angle(angle_degree)

        self.rotation = int(wrapped_angle)

    def wrap_angle(self, angle_):
        if angle_ < 0:
            return abs(angle_)
        else:
            return angle_

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