import numpy as np
from math import pi

class Marker():
    def __init__(self, marker_id, dataStrategy_):
        self.id = marker_id
        self.observers = []
        self.isVisible = False
        
        self.rotation = 0
        self.prev_rotation = 0
        self.rotation_threshold = pi/64
        
        self.center = (0,0)
        self.prev_center = (0,0)
        self.center_threshold = 2

        self.significant_change = False

        self.dataStrategy = dataStrategy_

    def found(self):
        self.isVisible = True

    def tracking(self, corners_):
        # check if it's a more significant change than the threshold
        self.rotation, self.center = self.check_for_threshold_change(corners_)
        self.notify_observers()

    def lost(self):
        self.isVisible = False
    

    def attach_observer(self, observer_):
        self.observers.append(observer_)
    
    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.build_json(), self.id)

    def get_id(self):
        return self.id
    
    def build_json(self):
        marker_data = {
            "id": self.id,
            "location": [self.center[0], self.center[1], 0],
            "rotation": self.rotation,
        }
        return marker_data
    
    def calculate_center(self, corners_):
        self.prev_center = self.center
        np_center = np.mean(corners_[0], axis=0)
        center = (int(np_center[0]), int(np_center[1]))
        return center
    
    def calculate_rotation(self, corners_):
        self.prev_rotation = self.rotation
        corners = corners_.reshape(-1,2)

        centroid = np.mean(corners, axis=0)

        reference_vector = corners[0] - centroid

        angle_rads = np.arctan2(reference_vector[1], reference_vector[0])
        angle_degree = np.degrees(angle_rads)

        # Subtract 45 degrees and wrap it to the -180 to 180 degree range
        adjusted_angle_degree = (angle_degree - 45 + 180) % 360 - 180
        adjusted_angle_radians = (angle_rads - pi/4 + pi) % (2 * pi) - pi

        return adjusted_angle_radians

    def check_for_threshold_change(self, corners):
        # calculate the rotation and center
        rotation = self.calculate_rotation(corners)
        center = self.calculate_center(corners)

        # check if it's a more significant change than the threshold
        # if it is, return the new values
        # if not, return the old values
        if abs(rotation - self.prev_rotation) >= self.rotation_threshold or abs(center[0] - self.prev_center[0]) >= self.center_threshold or abs(center[1] - self.prev_center[1]) >= self.center_threshold:
            self.significant_change = True
            return rotation, center
        else:
            self.significant_change = False
            return self.prev_rotation, self.prev_center