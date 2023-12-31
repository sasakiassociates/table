import argparse
import os
import threading

from cv2 import aruco

from detector import camera
from sender import repository
from ui import display
from detector import board
from detector import eventManager

FIREBASE_DEFAULT_URL = "https://magpietable-default-rtdb.firebaseio.com/"
ARUCO_DEFAULT_DICT = "6X6_100"
DEFAULT_MODE = "udp"

parser = argparse.ArgumentParser(description="Detect ArUco markers and send data to Firebase")

# ------------------------------------ Optional arguments ------------------------------------ #
parser.add_argument("--mode", type=str, default=DEFAULT_MODE, choices=["firebase", "udp", "both"], 
                    help="The mode to run the program in")
parser.add_argument("--url", type=str, default=FIREBASE_DEFAULT_URL, 
                    help="The path to the Firebase realtime database found in the Realtime Database tab of the Firebase project page")
parser.add_argument("--camera", type=int, default=0, 
                    help="The camera index to use")
parser.add_argument("--aruco_dict", type=str, default=ARUCO_DEFAULT_DICT, 
                    help="The name of the ArUco dictionary to use")
parser.add_argument("--video_full", type=bool, default=False, 
                    help="Whether or not to display the video in fullscreen")

# --------------------------------------- Debug arguments ------------------------------------ #
# To enable DEBUG mode, use the command `$env:DEBUG="True"` in PowerShell or `set DEBUG=True`  #
# in Command Prompt                                                                            #
# -------------------------------------------------------------------------------------------- #
DEBUG = True if os.environ.get("DEBUG") == "True" else False

args = parser.parse_args()

mode = args.mode
aruco_dict_name = args.aruco_dict

def camera_loop(camera, _display):
    while not _display.terminate:
        frame = camera.videoCapture()
        if frame is not None:
            _display.update_video_image(frame)
        else:
            print("Frame is None")
            break
        # _display.update_video_image(frame)

def camera_loop_fullscreen(camera, _display):
    while not _display.terminate:
        frame = camera.videoCapture()
        if frame is not None:
            _display.update_video_image_fullscreen(frame)
        else:
            print("Frame is None")
            break

if (__name__ == '__main__'):
    
    _display = display.Display()

    params = aruco.DetectorParameters()
    _event_manager = eventManager.EventManager()
    _repository = repository.Repository(mode, "test_proj", _event_manager)                  # New repository object that opens a UDP connection on a new thread
    board = board.Board(_repository, _event_manager)                           # New board object that uses the repository object to send data
    
    camera_num = args.camera

    camera = camera.Camera(camera_num, aruco_dict_name, params, board)  # New camera object that uses the repository object to send data and runs on it's own thread
    
    
    _event_manager.attach_observer(board)
    _event_manager.attach_observer(_repository)
    _event_manager.attach_observer(camera)
    
    if args.video_full == True:
        _display.build_video_fullscreen()
    else:
        _display.build()

    camera.setup()

    if args.video_full:
        camera_thread = threading.Thread(target=camera_loop_fullscreen, args=(camera, _display))
    else:
        camera_thread = threading.Thread(target=camera_loop, args=(camera, _display))
    camera_thread.daemon = True
    camera_thread.start()

    _display.launch_gui()

    board.end()
    camera.end()
    
# To Package:
# Use pyinstaller and include the following arguments:
# --onefile
# --icon="icon.ico"
# --add-data="projects;projects"
# --add-data="src/ui/ui_elements"
# --distpath /../dist
# --workpath /../build

# Example:
# pyinstaller --onefile --add-data="../projects;projects" --add-data="ui/ui_elements;ui_elements" --distpath ../dist --workpath ../build table-ui.py