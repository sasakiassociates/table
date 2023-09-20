import marker as m

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer):
        marker_list = []
        for i in range(dict_length):
            marker_list.append(m.Marker(i))
            marker_list[i].attach_observer(observer)

        return marker_list
    
    def get_num_project_files():
        import os
        import glob
        folder_path = os.path.join(os.getcwd(), "src", "projects")
        file_extension = '*.json' # We'll be using json to store information about the projects

        # Use glob to list files with the specified extension in the folder
        files = glob.glob(os.path.join(folder_path, file_extension))

        # Get the count of files
        num_files = len(files)
        return num_files