import json
import marker as m
import dataStrategy as ds

import os
import glob

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer):
        marker_list = []

        project_set_num = dict_length * 0.1                                     # 10% of the markers will be project markers
        controller_set_num = dict_length * 0.1                                  # 10% of the markers will be controller markers

        # First, let's make the project marker set
        num_projects = MarkerFactory.get_num_project_files()
        # These are the markers that correspond to existing projects
        for i in range(num_projects):
            marker_list.append(m.ProjectMarker(i, dataStrategy))
            marker_list[i].attach_observer(observer)
        for i in range(num_projects, project_set_num):
            dataStrategy = ds.NullDataStrategy()
            marker_list.append(m.ProjectMarker(i, dataStrategy))
            marker_list[i].attach_observer(observer)
        
        # Next, let's make the controller marker set
        for i in range(project_set_num, project_set_num + controller_set_num):
            marker_list.append(m.ControllerMarker(i, dataStrategy))
            marker_list[i].attach_observer(observer)

        # Finally, let's make the geometry marker set
        for i in range(project_set_num + controller_set_num, dict_length):
            marker_list.append(m.GeometryMarker(i, dataStrategy))
            marker_list[i].attach_observer(observer)

        return marker_list
    
    def get_num_project_files():
        folder_path = os.path.join(os.getcwd(), "..\\..\\projects")

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
    
if (__name__ == '__main__'):
    folder_path = os.path.join(os.getcwd(), "..\\..\\projects")
    json_files = MarkerFactory.get_json_files_sorted_by_creation_date(folder_path)

    for file_path in json_files:
        print(f"Content of {os.path.basename(file_path)}:")

        # Read and parse the JSON file
        with open(file_path, 'r') as json_file:
            project_data = json.load(json_file)
            print(json.dumps(project_data, indent=4))