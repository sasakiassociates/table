import json
import subprocess
from abc import ABC, abstractmethod
from math import pi
from uuid import uuid4 as uuid

import numpy as np
import cv2 as cv

@abstractmethod
class Marker(ABC):
    def __init__(self, marker_id, timer_):
        self.uuid = uuid()          # A unique identifier for this marker
        self.id = marker_id         # The id of the marker as detected by the camera corresponding to the aruco dictionary
        self.observers = []         # A list of observers that will be notified when the marker is updated
        self.is_visible = True
        self.gone = False
        
        self.rotation = 0
        self.prev_rotation = 0
        self.rotation_threshold = pi/40
        
        self.center = (0,0)
        self.prev_center = (0,0)
        self.center_threshold = 10

        self.timer = timer_         # A reference to the timer object so we can report when a marker is lost
        self.time_last_seen = None
        self.lost_threshold = 1000     # sets the time (in milliseconds) before a marker is considered lost

        # self.type = "marker"

        self.significant_change = False
        self.updated = False

    def found(self):
        self.is_visible = True
        self.timer.report_found(self)
        # self.notify_observers()

    def flip_center(self, width):
        return width - self.center[0], self.center[1]

    def update(self, corners_):
        # check if it's a more significant change than the threshold
        self.rotation, self.center = self.check_for_threshold_change(corners_)
        self.updated = True
        if self.significant_change:
            self.notify_observers()

    def lost(self):
        self.gone = True
        self.center = (0,0)
        self.rotation = 0
        self.notify_observers()

    def lost_tracking(self):
        self.is_visible = False
        self.timer.report_lost(self)
        
    def attach_observer(self, observer_):
        self.observers.append(observer_)
    
    def notify_observers(self):
        for observer in self.observers:
            if self.is_visible:
                observer.update(self.id, self.uuid, self.build_json())
            else:
                observer.remove_from_sent_data(self.id, self.uuid)

    def get_id(self):
        return self.id
    
    def build_json(self):
        marker_data = {
            "location": [-self.center[0], -self.center[1], 0],
            "rotation": self.rotation,
        }
        return marker_data
    
    def calculate_center(self, corners_):
        np_center = np.mean(corners_[0], axis=0)
        center = (int(np_center[0]), int(np_center[1]))
        return center
    
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
            
            self.significant_change = True
            return rotation, center                         # if it is, return the new values
        else:
            self.significant_change = False
            return self.prev_rotation, self.prev_center     # if not, return the old values
        
    def draw(self, frame):
        # Flip center
        draw_center = self.flip_center(frame.shape[1])
        # Draw the center
        cv.circle(frame, draw_center, 5, (0, 0, 255), -1)
        # Draw the id
        cv.putText(frame, str(self.id), draw_center, cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2, cv.LINE_AA)
        # Draw the rotation
        cv.putText(frame, str(round(self.rotation, 2)), (draw_center[0], draw_center[1] + 30), cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2, cv.LINE_AA)
        # Draw the uuid
        cv.putText(frame, str(self.uuid), (draw_center[0], draw_center[1] + 60), cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2, cv.LINE_AA)
        
class ProjectMarker(Marker):
    def __init__(self, marker_id, timer_):
        super().__init__(marker_id, timer_)
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
    
class GenericMarker(Marker):
    def __init__(self, marker_id, timer_):
        super().__init__(marker_id, timer_)


if (__name__ == '__main__'):
    print("Running unit tests for marker.py")
    
    print("Unit tests for marker.py passed")