import json
from . import marker as m
# import marker as m

import os
import glob

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer):
        marker_list = []

        project_set_num = int(dict_length * 0.1)                        # 10% of the markers will be project markers
        controller_set_num = int(dict_length * 0.1)                     # 10% of the markers will be controller markers
        
        # First, let's make the camera marker
        marker_list.append(m.ControllerMarker(0))
        marker_list[0].attach_observer(observer)
        marker_list[0].set_controller_type("camera")

        # Next, let's make the controller marker set
        for i in range(1, controller_set_num):
            marker_list.append(m.ControllerMarker(i))
            marker_list[i].attach_observer(observer)
        
        # Next, let's make the project marker set (making sure they are not the same as the controller marker ids)
        # json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder
        json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder

        project_marker_ids = []
        for file in json_files:
            marker_id = int(file['marker_id'])
            if marker_id not in range(controller_set_num) and marker_id < dict_length:
                project_marker_ids.append(marker_id)           # Build a list of all the marker ids associated with a project file
            elif marker_id in range(controller_set_num):
                print(f"ERROR: {file['name']} has the same ID ({marker_id}) as a controller marker.")
                print(f"IDs 0 through {controller_set_num - 1} are reserved for controller markers. Please change the ID of this project.")
            else:
                print(f"ERROR: {file['name']} has an ID ({marker_id}) that is out of range for this dictionary. The highest id in this dictionary is {dict_length - 1}. Please change the ID of this project.")

        for marker_id in project_marker_ids:
            marker_id = int(marker_id)
            NewMarker = m.ProjectMarker(marker_id)
            NewMarker.attach_observer(observer)
            for file in json_files:                                     # If a marker id matches the associated marker id of a project file, associate the project file with the marker
                if int(file['marker_id']) == marker_id:
                    NewMarker.associate_marker_with_project(file)
                    break
            marker_list.append(NewMarker)              # Make a project marker for each marker id associated with a project file

        # Finally, let's make the geometry marker set
        for i in range(controller_set_num, dict_length):
            if i not in project_marker_ids:
                marker_list.append(m.GeometryMarker(i))
                marker_list[i].attach_observer(observer)
                marker_list[i].name = "Geometry {i}"

        marker_list.sort(key=lambda marker: marker.id) # Sort the markers by their id

        return marker_list
    
    def get_num_project_files():
        folder_path = os.path.join(os.getcwd(), "..\\projects")

        file_extension = '*.json' # We'll be using json to store information about the projects

        # Use glob to list files with the specified extension in the folder
        files = glob.glob(os.path.join(folder_path, file_extension))

        # Get the count of files
        num_files = len(files)
        return num_files
    
    # NOTE currently we sort the projects by creation date and don't require a specific naming convention
    def get_json_files_sorted_by_creation_date(folder_path):
        file_extension = '*.json'
        json_files = glob.glob(os.path.join(folder_path, file_extension))

        # Sort the JSON files by their creation date (oldest to newest)
        json_files.sort(key=lambda file: os.path.getctime(file))
        return json_files
    
    @staticmethod
    def load_json_files(relative_project_path):
        json_objects = []
        try:
            for filename in os.listdir(os.path.join(os.getcwd(), relative_project_path)):
                file_path = os.path.join(os.getcwd(), relative_project_path, filename)

                if filename.endswith(".json") and os.path.isfile(file_path):
                    with open(os.path.join(os.getcwd(), relative_project_path, filename), 'r') as json_file:
                        json_objects.append(json.load(json_file))
        except Exception as e:
            print("Error reading project files")
            print(e)
        return json_objects
    
if (__name__ == '__main__'):
    marker_list = MarkerFactory.make_markers(100, None)
    for marker in marker_list:
        marker_type = type(marker).__name__
        if marker_type == "ProjectMarker":
            print(f"Marker ID: {marker.id} | Marker Type: {marker_type} | Project Name: {marker.project_name}")
        else:
            print(f"Marker ID: {marker.id} | Marker Type: {marker_type}")

    # Simulate Marker 0 being detected and open Project 1
    # marker_list[0].open_project()