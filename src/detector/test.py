import glob
import os

folder_path = 'C:\\Users\\nshikada\\Documents\\GitHub\\table\\src\\detector'
file_extension = '*.json'  # Change this to match the extension of the files you want to count

# Use glob to list files with the specified extension in the folder
files = glob.glob(os.path.join(folder_path, file_extension))

# Get the count of files
num_files = len(files)

print(f"Number of {file_extension} files in the folder: {num_files}")