import json
from . import marker as m
from . import zone as z
# import marker as m

import os
import glob

BOUNDING_ZONE_IDS = ()

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer, timer_):
        marker_list = []

        # First, let's make the project marker set (making sure they are not the same as the controller marker ids)
        # json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder
        json_files = MarkerFactory.load_json_files("..\\projects")  # Find all files in the project folder

        assigned_marker_ids = []

        if len(json_files) > 0:
            for file in json_files:
                marker_id = int(file['marker_id'])
                if marker_id > 1 & marker_id < dict_length:
                    assigned_marker_ids.append(marker_id)           # Build a list of all the marker ids associated with a project file
                elif marker_id == 0:
                    print(f"ERROR: {file['name']} has an ID ({marker_id}) that is reserved for camera control. Please change the ID of this project.")
                else:
                    print(f"ERROR: {file['name']} has an ID ({marker_id}) that is out of range for this dictionary. The highest id in this dictionary is {dict_length - 1}. Please change the ID of this project.")
            for marker_id in assigned_marker_ids:
                marker_id = int(marker_id)
                new_project_marker = m.ProjectMarker(marker_id, timer_)
                new_project_marker.attach_observer(observer)
                for file in json_files:                                     # If a marker id matches the associated marker id of a project file, associate the project file with the marker
                    if int(file['marker_id']) == marker_id:
                        new_project_marker.associate_marker_with_project(file)
                        break
                marker_list.append(new_project_marker)              # Make a project marker for each marker id associated with a project file

        # Next, let's make the bounding zone
        if BOUNDING_ZONE_IDS == ():
            print("No zone defined, skipping zone creation")
            bounding_zone = None
        else:
            bounding_zone = z.Zone('model_space', timer_)
            for marker_id in BOUNDING_ZONE_IDS:
                if marker_id not in assigned_marker_ids:
                    marker = m.GenericMarker(marker_id, timer_)
                    bounding_zone.add_marker(marker)
                    assigned_marker_ids.append(marker_id)
                    marker_list.append(marker)
                    print(f"Created marker {marker_id} for bounding zone")
                else:
                    print(f"ERROR: Marker ID {marker_id} is already assigned to project {marker_list[marker_id].project_name}. Please change the ID of this project.")
            bounding_zone.attach_observer(observer)

        # Finally, let's make the rest of the markers generic markers
        for i in range(0, dict_length):
            if i not in assigned_marker_ids:
                marker = m.GenericMarker(i, timer_)
                marker.attach_observer(observer)
                marker_list.append(marker)
                assigned_marker_ids.append(i)

        marker_list[0].type = "camera"

        # Organize the list in order of marker id
        marker_list.sort(key=lambda marker: marker.id)

        print(f"Created {len(assigned_marker_ids)} markers")

        # for marker in marker_list:
        #     print(f"Marker ID: {marker.id} | Marker Type: {marker.type}")

        marker_list.sort(key=lambda marker: marker.id) # Sort the markers by their id

        return marker_list, bounding_zone
    
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