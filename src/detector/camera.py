import sys
import traceback

import cv2 as cv
import numpy as np
from cv2 import aruco

from . import markerFactory as factory
from . import marker as m
from . import arucoReference as ar
from . import timer as t

class Camera():
    def __init__(self, camera_num, aruco_dict_name, params, repository_):
        self.repository = repository_

        if aruco_dict_name in ar.aruco_dict_mapping:
            aruco_dict = aruco.getPredefinedDictionary(ar.aruco_dict_mapping[aruco_dict_name])
        else:
            aruco_dict = aruco.getPredefinedDictionary(ar.aruco_dict_mapping['DICT_6X6_100'])
            print("Invalid ArUco dictionary name. Using default dictionary: DICT_6X6_100")

        dictionary_length = len(aruco_dict.bytesList)
        self.timer = t.Timer()
        self.timer.start()
        self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_, self.timer)
        self.detector = aruco.ArucoDetector(aruco_dict, params)
        
        self.cap = cv.VideoCapture(camera_num, cv.CAP_DSHOW)
        self.cap.set(cv.CAP_PROP_FRAME_WIDTH, 1080)
        self.cap.set(cv.CAP_PROP_FRAME_HEIGHT, 720)

        self.background_color = (0, 0, 0)

        if not self.cap.isOpened():
            print("Cannot open camera")
            exit()
            
    """
    Loop through the markers and update them
    """
    def videoCapture(self):
        try:
            ret, frame = self.cap.read()
            if ret:
                frame_gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

                # Detect the markers
                corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                # frame_marked = aruco.drawDetectedMarkers(frame_gray, corners, ids)

                # Flip image
                frame_gray = cv.flip(frame_gray, 1)

                frame_color = cv.cvtColor(frame_gray, cv.COLOR_GRAY2BGR)

                # background = np.zeros((frame_color.shape[0], frame_color.shape[1], 3), dtype=np.uint8)
                # frame_color = cv.addWeighted(frame_color, 0.2, background, 0.8, 0.0)

                # cv.rectangle(frame_color, (0, 0), (frame_color.shape[1], frame_color.shape[0]), self.background_color, -1)

                # Loop through the markers and update them
                #self.markerLoop(ids, corners)
                if ids is not None:
                    for marker_id, marker_corners in zip(ids, corners):
                        marker = self.my_markers[int(marker_id)]

                        if isinstance(marker, m.ProjectMarker):
                            if marker.running == False:
                                marker.open_project()
                        else:
                            if marker.is_visible == False:
                                marker.found()
                                marker.track(marker_corners)
                                marker.flip_center(frame_color.shape[1])
                            else:
                                marker.track(marker_corners)
                                marker.flip_center(frame_color.shape[1])
                            
                            x = marker.center[0]
                            y = marker.center[1]
                            radius = 20
                            color = (193, 76, 255)
                            cv.ellipse(frame_color, (int(x), int(y)), (radius, radius), 0, 0, 360, color, 3)
                            cv.putText(frame_color, str(marker.id), (int(x+radius*1.25), int(y+radius/2)), cv.FONT_HERSHEY_SIMPLEX, 1, color, 2, cv.LINE_AA)
                    for marker in self.my_markers:
                        if marker.is_visible == True and marker.id not in ids:
                            marker.lost_tracking()
                            marker.is_visible = False
                else:
                    for marker in self.my_markers:
                        if marker.is_visible == True:
                            marker.lost_tracking()
                            marker.is_visible = False
                            
                if self.repository.new_data:
                    self.repository.strategy.send()
                    self.repository.new_data = False

                return frame_color
        except Exception as e:
            sys.stderr.write(str(e))
            traceback.print_exc()

    def end(self):
        self.cap.release()
        cv.destroyAllWindows()
        self.timer.stop()

if (__name__ == '__main__'):
    print("Running unit tests for camera.py")
    camera = Camera(0, None, None, None)
    while True:
        frame = camera.videoCapture()
        cv.imshow('frame', frame)
        if cv.waitKey(1) == ord('q'):
            break
    print("Unit tests for camera.py passed")