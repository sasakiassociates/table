from abc import ABC, abstractmethod
import json
import os
import numpy as np
from math import pi

@abstractmethod
class Marker(ABC):
    def __init__(self, marker_id):
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
        if abs(rotation - self.prev_rotation) >= self.rotation_threshold or abs(center[0] - self.prev_center[0]) >= self.center_threshold or abs(center[1] - self.prev_center[1]) >= self.center_threshold:
            self.significant_change = True
            return rotation, center                         # if it is, return the new values
        else:
            self.significant_change = False
            return self.prev_rotation, self.prev_center     # if not, return the old values
        
class ProjectMarker(Marker):
    def __init__(self, marker_id):
        super().__init__(marker_id)
        self.project_name = ""
        self.project_author = ""
        self.project_files = {}
        self.project_date = ""
        self.project_path = ""

    # Intakes the project path associated with this marker and parses the json file to build out this marker
    def associate_marker_with_project(self, project_path):
        self.project_path = project_path
        # Read and parse the JSON file
        try:
            with open(self.project_path, 'r') as json_file:
                project_data = json.load(json_file)
                self.project_name = project_data["name"]
                self.project_author = project_data["author"]
                self.project_files = project_data["files"]
                self.project_date = project_data["date"]
        except Exception as e:
            print(f"Error reading project file {os.path.basename(project_path)}")
            print(e)

    # Sample JSON file:
    # {
    #     "name": "Project 1",
    #     "author": "John Doe",
    #     "files": {
    #         "rhino": "C:\\Users\\JohnDoe\\Documents\\Rhino\\Project1.3dm",
    #         "grasshopper": "C:\\Users\\JohnDoe\\Documents\\Grasshopper\\Project1.gh"
    #         "unity": "C:\\Users\\JohnDoe\\Documents\\Unity\\Project1\\Project1.unitypackage"
    #     },
    #     "date": "2021-01-01"
    # }

    # Save the json file to the project folder and then associate that file with this marker
    def save_project(self, json_data):
        with open("..\\..\\projects", 'w') as json_file:
            json.dump(json_data, json_file, indent=4)
        self.associate_marker_with_project(self.project_path)

    def build_json(self):
        marker_data = super().build_json()
        return marker_data

    # TODO implement this
    def open_project(self):
        for file in self.project_files:
            try:
                if file.endswith(".3dm"):
                    pass
                elif file.endswith(".gh"):
                    pass
                elif file.endswith(".unitypackage"):
                    pass
                else:
                    print(f"File type {file} not supported")
                    return
            except Exception as e:
                print(f"Did not find file: {file}")
                print(e)
    
class ControllerMarker(Marker):
    def __init__(self, marker_id):
        super().__init__(marker_id)
        self.controller_name = ""
        self.controller_data = {}

    def build_json(self):
        marker_data = super().build_json()
        marker_data["controller_name"] = self.controller_name
        marker_data["controller_data"] = self.controller_data
        return marker_data
    
class GeometryMarker(Marker):
    def __init__(self, marker_id):
        super().__init__(marker_id)
        self.geometry_name = ""
        self.geometry_data = {}

    def build_json(self):
        marker_data = super().build_json()
        marker_data["geometry_name"] = self.geometry_name
        marker_data["geometry_data"] = self.geometry_data
        return marker_data
    
if (__name__ == '__main__'):
    print("Running unit tests for marker.py")

    print("Unit tests for marker.py passed")