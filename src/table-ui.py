#from detector import *
from ui import display
from detector import camera
import threading

def camera_loop(camera, display):
    while not display.terminate:
        frame = camera.videoCapture()
        display.update_video_image(frame)

if __name__ == "__main__":
    camera = camera.Camera(0, None, None, None)
    display = display.Display()
    display.build()
    display.register_video_image((640, 480))

    camera_thread = threading.Thread(target=camera_loop, args=(camera, display))
    camera_thread.start()

    display.launch_gui()

    camera.cap.release()