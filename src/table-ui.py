#from detector import *
from ui import display
from detector import camera
import threading

def camera_loop(camera, _display):
    while not _display.terminate:
        frame = camera.videoCapture()
        _display.update_video_image(frame)

# TODO implement threading "Queues" to pass data between threads
if (__name__ == '__main__'):
    camera = camera.Camera(0, None, None, None)
    _display = display.Display()
    
    _display.build()
    _display.register_video_label((940, 720))

    camera_thread = threading.Thread(target=camera_loop, args=(camera, _display))
    camera_thread.daemon = True
    camera_thread.start()

    _display.launch_gui()
    camera.cap.release()
    _display.root.destroy()