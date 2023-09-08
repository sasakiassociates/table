import numpy as np
from math import pi

#TODO get rid of inherited markers, we only need the one since assignment is done in the other app
class Marker():
    def __init__(self, marker_id_):
        self.id = marker_id_
        self.observers = []
        self.rotation = 0
        self.type = ""

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
    
    def calculate_center(self, corners_):
        np_center = np.mean(corners_[0], axis=0)
        self.center = (int(np_center[0]), int(np_center[1]))
    
    def calculate_rotation(self, corners_):
        corners = corners_.reshape(-1,2)

        centroid = np.mean(corners, axis=0)

        reference_vector = corners[0] - centroid

        angle_rads = np.arctan2(reference_vector[1], reference_vector[0])
        angle_degree = np.degrees(angle_rads)

        # Subtract 45 degrees and wrap it to the -180 to 180 degree range
        adjusted_angle_degree = (angle_degree - 45 + 180) % 360 - 180
        adjusted_angle_radians = (angle_rads - pi/4 + pi) % (2 * pi) - pi

        self.rotation = adjusted_angle_radians

    def set_type(self, type_):
        self.type = type_