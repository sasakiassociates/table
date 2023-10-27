import json
from . import marker as m
# import marker as m
from . import zone as z

import os
import glob

# Static variables
CAMERA_MARKER_ID = 0
NUM_OF_ZONES = 1
NUM_MARKERS_PER_ZONE = 3

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, repository_, timer_):
        marker_list = []

        # First, let's make the project marker set (making sure they are not the same as the controller marker ids)
        # json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder
        json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder

        assigned_marker_ids = []

        if len(json_files) > 0:
            for file in json_files:
                marker_id = int(file['marker_id'])
                if marker_id == CAMERA_MARKER_ID:
                    print(f"ERROR: {file['name']} has an ID ({marker_id}) that is reserved for camera control. Please change the ID of this project.")
                elif marker_id >= 0 & marker_id < dict_length:
                    id_ = int(file['marker_id'])
                    new_project_marker = m.ProjectMarker(id_, timer_)
                    new_project_marker.attach_observer(repository_)
                    new_project_marker.associate_marker_with_project(file)
                    marker_list.append(new_project_marker)

                    assigned_marker_ids.append(marker_id)                       # Add to the list of already assigned marker ids
                else:
                    print(f"ERROR: {file['name']} has an ID ({marker_id}) that is out of range for this dictionary. The highest id in this dictionary is {dict_length - 1}. Please change the ID of this project.")

        # Now let's make the zones
        # TODO change from the default of 10 zones to a user defined number
        for i in range(0, NUM_OF_ZONES):
            end_of_dict = dict_length
            # Check to make sure it hasn't already been assigned
            # Make the zone
            zone_ = z.Zone()

            marker_id_start = end_of_dict - ((i+1) * NUM_MARKERS_PER_ZONE)
            # Make the markers that will be in the zone
            for j in range(0, NUM_MARKERS_PER_ZONE):
                id_ = marker_id_start + j
                if id_ in assigned_marker_ids:
                    # This assumes it's a project, maybe we should check the type of marker first
                    conflicting_project = marker_list[assigned_marker_ids.index(i)].project_name
                    print(f"ERROR: Zone {i} has already been assigned to a project. Please change the ID of project: {conflicting_project}.")
                marker_ = m.GenericMarker(id_, timer_)
                marker_.attach_observer(zone_)
                marker_list.append(marker_)
                print(f"Created marker {id_} in zone {i}")
                assigned_marker_ids.append(id_)

        # Finally, let's make the rest of the markers generic markers
        for i in range(0, dict_length):
            if i not in assigned_marker_ids:
                marker_list.append(m.GenericMarker(i, timer_))
                marker_list[i].attach_observer(repository_)
        
        marker_list[CAMERA_MARKER_ID].type = "camera"

        print(f"Created {len(marker_list)} markers")

        # for marker in marker_list:
        #     print(f"Marker ID: {marker.id} | Marker Type: {marker.type}")

        marker_list.sort(key=lambda marker: marker.id) # Sort the markers by their id

        return marker_list
    
    """
    Returns the number of project files in the projects folder
    """
    def get_num_project_files():
        folder_path = os.path.join(os.getcwd(), "..\\projects")

        file_extension = '*.json' # We'll be using json to store information about the projects

        # Use glob to list files with the specified extension in the folder
        files = glob.glob(os.path.join(folder_path, file_extension))

        # Get the count of files
        num_files = len(files)
        return num_files
    
    """
    Returns a list of the json files in the projects folder sorted by their creation date (oldest to newest)
    """
    # NOTE currently we sort the projects by creation date and don't require a specific naming convention
    def get_json_files_sorted_by_creation_date(folder_path):
        file_extension = '*.json'
        json_files = glob.glob(os.path.join(folder_path, file_extension))

        # Sort the JSON files by their creation date (oldest to newest)
        json_files.sort(key=lambda file: os.path.getctime(file))
        return json_files
    
    @staticmethod
    def load_json_files(relative_project_path):
        if not os.path.exists(os.path.join(os.getcwd(), relative_project_path)):
            print("Project folder does not exist")
            return []
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