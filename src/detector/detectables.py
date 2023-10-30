import json
import subprocess
from abc import ABC, abstractmethod
from math import pi
import uuid

import numpy as np

@abstractmethod
class DetectableObject(ABC):
    def __init__(self, object_id, timer_):
        self.uuid = uuid.uuid4()
        self.id = object_id
        self.observers = []
        self.is_visible = False
        
        self.timer = timer_         # A reference to the timer object so we can report when a marker is lost
        self.time_last_seen = None
        self.lost_threshold = 500     # sets the time (in milliseconds) before a marker is considered lost

        self.type = "generic"

        self.significant_change = False

        self.display_color = (0, 0, 0)

    def found(self):
        self.is_visible = True
        self.timer.report_found(self)
        self.notify_observers()

    def track(self, corners_):
        # check if it's a more significant change than the threshold
        self.check_for_threshold_change(corners_)
        if self.significant_change:
            self.notify_observers()

    def lost(self):
        self.is_visible = False
        self.center = (0,0)
        self.rotation = 0
        for observer in self.observers:
            observer.lost_subject(self.uuid)
        # self.notify_observers()

    def lost_tracking(self):
        self.timer.report_lost(self)
        
    def attach_observer(self, observer_):
        self.observers.append(observer_)
    
    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.uuid, self.build_json())
    
    @abstractmethod
    def build_json(self):
        pass
    
class ProjectMarker(DetectableObject):
    def __init__(self, object_id, timer_):
        super().__init__(object_id, timer_)
        self.project_name = ""
        self.project_author = ""
        self.project_files = {}
        self.project_date = ""
        self.associated = False
        self.running = False
        self.type = "project"

    # Intakes the project path associated with this marker and parses the json file to build out this marker
    def associate_marker_with_project(self, json_data):
        self.project_name = json_data["name"]
        self.project_author = json_data["author"]
        self.project_files = json_data["files"]
        self.project_date = json_data["date"]
        self.associated = True

    # Save the json file to the project folder and then associate that file with this marker
    def save_project(self, json_data):
        filename = json_data["name"] + ".json"
        with open(f"..\\..\\projects\\{filename}", 'w') as json_file:
            json.dump(json_data, json_file, indent=4)
        self.associate_marker_with_project(self.project_path)

    # TODO make sure certain types are opened after others (i.e. grasshopper after rhino)
    def open_project(self):
        rhino_path = None
        grasshopper_path = None
        file_path = None

        for filetype in self.project_files:     # Build a list of all the files in the project
            try:
                if filetype == "rhino":
                    rhino_path = self.project_files[filetype]
                elif filetype == "grasshopper":
                    grasshopper_path = self.project_files[filetype]
                else:
                    file_path = self.project_files[filetype]
            except Exception as e:
                print(f"Did not find file: {self.project_files[filetype]}")
                print(e)
        
        if rhino_path and grasshopper_path:
            try:
                program_path = "C:\\Program Files\\Rhino 7\\System\\Rhino.exe"
                runscript_command = f'''_-RunScript (Set GH = Rhino.GetPlugInObject(""Grasshopper"")) _-RunScript (Call GH.OpenDocument(""{grasshopper_path}"")))'''
                app = subprocess.Popen(f'"{program_path}" "{rhino_path}" /nosplash /notemplate /runscript="{runscript_command}"', shell=True)
                self.running = True
                # TODO add a way to kill the program if a new project is detected
            except Exception as e:
                print(f"Failed to open rhino and grasshopper: {e}")
        elif rhino_path:
            try:
                program_path = "C:\\Program Files\\Rhino 7\\System\\Rhino.exe"
                subprocess.Popen(f'"{program_path}" "{rhino_path}" /nosplash /notemplate', shell=True)
                self.running = True
            except Exception as e:
                print(f"Failed to open rhino: {e}")
        elif grasshopper_path:
            try:
                program_path = "C:\\Program Files\\Rhino 7\\System\\Rhino.exe"
                runscript_command = f'''_-RunScript (Set GH = Rhino.GetPlugInObject(""Grasshopper"")) _-Runscript (Call GH.OpenDocument(""{grasshopper_path}""))'''
                subprocess.Popen(f'"{program_path}" /nosplash /notemplate /runscript="{runscript_command}"', shell=True)
                self.running = True
            except Exception as e:
                print(f"Failed to open grasshopper: {e}")
        else:
            print("Failed to open project")
    
class Marker(DetectableObject):
    def __init__(self, object_id, timer_):
        super().__init__(object_id, timer_)
        self.type = "marker"

        self.rotation = 0
        self.prev_rotation = 0
        self.rotation_threshold = pi/32
        
        self.center = (0,0)
        self.prev_center = (0,0)
        self.center_threshold = 10

    def build_json(self):
        marker_data = {
            "id": self.id,
            "location": [-self.center[0], -self.center[1], 0],
            "rotation": self.rotation,
            "type": self.type,
        }
        return marker_data
    
    def calculate_center(self, corners_):
        np_center = np.mean(corners_[0], axis=0)
        center = (int(np_center[0]), int(np_center[1]))
        return center
    
    def flip_center(self, width):
        self.center = (width - self.center[0], self.center[1])
    
    def calculate_rotation(self, corners_):
        corners = corners_.reshape(-1,2)

        centroid = np.mean(corners, axis=0)

        reference_vector = corners[0] - centroid

        angle_rads = np.arctan2(reference_vector[1], reference_vector[0])
        angle_degree = np.degrees(angle_rads)

        # Subtract 45 degrees and wrap it to the -180 to 180 degree range
        adjusted_angle_radians = (angle_rads - pi/4 + pi) % (2 * pi) - pi

        return adjusted_angle_radians

    def check_for_threshold_change(self, corners):
        # calculate the rotation and center
        rotation = self.calculate_rotation(corners)
        center = self.calculate_center(corners)

        # check if it's a more significant change than the threshold
        if abs(rotation - self.prev_rotation) >= self.rotation_threshold or abs(center[0] - self.prev_center[0]) >= self.center_threshold or abs(center[1] - self.prev_center[1]) >= self.center_threshold:
            self.prev_rotation = rotation
            self.prev_center = center
            
            self.rotation = rotation
            self.center = center
            self.significant_change = True
        else:
            self.rotation = self.prev_rotation
            self.center = self.prev_center
            self.significant_change = False

class Zone(DetectableObject):
    def __init__(self, object_id, timer_):
        super().__init__(object_id, timer_)
        self.type = "zone"
        self.markers = []

    def add_marker(self, marker_):
        self.markers.append(marker_)

    def remove_marker(self, marker_):
        self.markers.remove(marker_)

    def update(self, uuid_, data_):
        # Check if all the markers are visible
        if self.check_for_threshold_change():
            self.significant_change = True
            self.calculate_bounds()
            self.notify_observers()
            self.is_visible = True
        else:
            self.significant_change = False
            self.is_visible = False
        
    def lost_subject(self, uuid_):
        for observer in self.observers:
            observer.lost_subject(uuid_)
            self.is_visible = False

    def build_json(self):
        zone_data = {
            "name": self.id,
            "points": self.bounding_box,
            "type": self.type,
            "color": self.display_color
        }
        return zone_data
    
    def calculate_bounds(self):
        min_x = min(self.markers, key=lambda m: m.center[0]).center[0]
        min_y = min(self.markers, key=lambda m: m.center[1]).center[1]
        max_x = max(self.markers, key=lambda m: m.center[0]).center[0]
        max_y = max(self.markers, key=lambda m: m.center[1]).center[1]

        self.bounding_box = (min_x, min_y, max_x, max_y)

    def check_for_threshold_change(self):
        for marker in self.markers:
            # We need to check if all the markers are visible
            if marker.is_visible == False:
                return False
            # We need to check if any of the markers have a significant change
            if marker.significant_change:
                return True
        return False

if (__name__ == '__main__'):
    print("Running unit tests for marker.py")
    
    print("Unit tests for marker.py passed")