#from detector import *
from ui import display
from detector import camera
import threading

def camera_loop(camera, display):
    while not display.terminate:
        if display.running:
            frame = camera.videoCapture()

            display.lock.acquire()
            display.update_video_image(frame)
            display.lock.release()

if (__name__ == '__main__'):
    camera = camera.Camera(0, None, None, None)
    display = display.Display()
    display.build()
    display.register_video_image((940, 720))

    camera_thread = threading.Thread(target=camera_loop, args=(camera, display))
    camera_thread.start()

    display.launch_gui()

    display.root.destroy()
    camera_thread.join()
    camera.cap.release()