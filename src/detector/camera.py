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
        timer = t.Timer()
        self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_, timer)
        self.detector = aruco.ArucoDetector(aruco_dict, params)
        
        self.cap = cv.VideoCapture(camera_num, cv.CAP_DSHOW)
        self.cap.set(cv.CAP_PROP_FRAME_WIDTH, 1080)
        self.cap.set(cv.CAP_PROP_FRAME_HEIGHT, 720)

        if not self.cap.isOpened():
            print("Cannot open camera")
            exit()
            
        self.changed_data = False

    # TODO rework this so the while loop is in the main.py file so we don't need to add things to this function to add functionality. Also make it return the frame
    def videoCapture(self):
        try:
            ret, frame = self.cap.read()
            if ret:
                frame_gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

                # Detect the markers
                corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                frame_marked = aruco.drawDetectedMarkers(frame_gray, corners, ids)

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
                                self.changed_data = True
                            else:
                                marker.track(marker_corners)
                                if marker.significant_change:
                                    self.changed_data = True
                    for marker in self.my_markers:
                        if marker.is_visible == True and marker.id not in ids:
                            marker.lost()
                            self.changed_data = True
                else:
                    for marker in self.my_markers:
                        if marker.is_visible == True:
                            marker.lost()
                            # self.changed_data = True
                            
                if self.changed_data:
                    self.repository.send_data()
                    self.changed_data = False

                return frame_marked
        except Exception as e:
            sys.stderr.write(str(e))
            traceback.print_exc()

if (__name__ == '__main__'):
    print("Running unit tests for camera.py")
    camera = Camera(0, None, None, None)
    while True:
        frame = camera.videoCapture()
        cv.imshow('frame', frame)
        if cv.waitKey(1) == ord('q'):
            break
    print("Unit tests for camera.py passed")