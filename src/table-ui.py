import argparse
import threading

from cv2 import aruco

from detector import camera
from ui import display

parser = argparse.ArgumentParser(description="Detect ArUco markers and send data to Firebase")
parser.add_argument("mode", type=str, default="udp", choices=["firebase", "udp"], help="The mode to run the program in")
parser.add_argument("--url", type=str, default="https://magpietable-default-rtdb.firebaseio.com/", help="The path to the Firebase realtime database found in the Realtime Database tab of the Firebase project page")
parser.add_argument("--camera", type=int, default=0, help="The camera index to use")
parser.add_argument("--aruco_dict", type=str, default="DICT_6X6_100", help="The name of the ArUco dictionary to use")

def camera_loop(camera, _display):
    while not _display.terminate:
        frame = camera.videoCapture()
        _display.update_video_image(frame)

if (__name__ == '__main__'):

    aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)
    params = aruco.DetectorParameters()
    
    camera = camera.Camera(0, aruco_dict, params, None)
    _display = display.Display()
    
    _display.build()

    camera_thread = threading.Thread(target=camera_loop, args=(camera, _display))
    camera_thread.daemon = True
    camera_thread.start()

    _display.launch_gui()
    camera.cap.release()
    _display.root.destroy()