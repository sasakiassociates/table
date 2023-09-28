import argparse
import os
import threading

from cv2 import aruco

from detector import camera
from sender import repository
from ui import display

FIREBASE_DEFAULT_URL = "https://magpietable-default-rtdb.firebaseio.com/"
ARUCO_DEFAULT_DICT = "DICT_6X6_100"

parser = argparse.ArgumentParser(description="Detect ArUco markers and send data to Firebase")

# ------------------------------------ Required arguments ------------------------------------ #
parser.add_argument("mode", type=str, default="udp", choices=["firebase", "udp"], 
                    help="The mode to run the program in")

# ------------------------------------ Optional arguments ------------------------------------ #
parser.add_argument("--url", type=str, default=FIREBASE_DEFAULT_URL, 
                    help="The path to the Firebase realtime database found in the Realtime Database tab of the Firebase project page")
parser.add_argument("--camera", type=int, default=0, 
                    help="The camera index to use")
parser.add_argument("--aruco_dict", type=str, default=ARUCO_DEFAULT_DICT, 
                    help="The name of the ArUco dictionary to use")

# --------------------------------------- Debug arguments ------------------------------------ #
# To enable DEBUG mode, use the command `$env:DEBUG="True"` in PowerShell or `set DEBUG=True`  #
# in Command Prompt                                                                            #
# -------------------------------------------------------------------------------------------- #
DEBUG = True if os.environ.get("DEBUG") == "True" else False

args = parser.parse_args()

aruco_dict_name = args.aruco_dict

def camera_loop(camera, _display):
    while not _display.terminate:
        frame = camera.videoCapture()
        _display.update_video_image(frame)

if (__name__ == '__main__'):
    
    _display = display.Display()

    aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_6X6_100)
    params = aruco.DetectorParameters()
    _repository = repository.Repository("udp")                  # New repository object that opens a UDP connection on a new thread
    
    camera = camera.Camera(0, aruco_dict_name, params, _repository)  # New camera object that uses the repository object to send data and runs on it's own thread
    
    _display.build()
    
    if DEBUG:
        print("Running in debug mode")
        # TODO add a window for debug mode that allows you to input markers manually as if they had been detected
        # window just links up to the repository object and allows you to input data for a marker
        # window also allows you to input data for a project

        _display.add_debug_window()
    
    else:

        camera_thread = threading.Thread(target=camera_loop, args=(camera, _display))
        camera_thread.daemon = True
        camera_thread.start()

        _display.launch_gui()
        camera.cap.release()
        _display.root.destroy()
        