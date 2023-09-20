import sys
import traceback

import cv2 as cv
import numpy as np
from cv2 import aruco

import threading

#import markerFactory as factory

class Camera():
    def __init__(self, camera_num, aruco_dict, params, repository_):
        self.repository = repository_

#        dictionary_length = len(aruco_dict.bytesList)
        #self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_)
        #self.project_markers = factory.MarkerFactory.make_project_markers()
        #self.detector = aruco.ArucoDetector(aruco_dict, params)
        
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
                #corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                #frame_marked = aruco.drawDetectedMarkers(frame_gray, corners, ids)

                # Loop through the markers and update them
                #self.markerLoop(ids, corners)

                if self.changed_data:
                    self.repository.send_data()
                    self.changed_data = False

                #cv.imshow('frame', frame_marked)

                return frame_gray
        except Exception as e:
            sys.stderr.write(str(e))
            traceback.print_exc()

    '''
    Loop through the markers and update them
    
    @param ids: the ids of the markers
    @param corners: the corners of the markers
    '''
    def markerLoop(self, ids, corners):
        for marker in self.my_markers:
            if marker.id in ids & marker.isVisible == False:
                marker.found()
            elif marker.id in ids & marker.isVisible == True:
                marker.tracking(corners[ids == marker.id])
                if marker.significant_change:
                    self.changed_data = True
            elif marker.id not in ids & marker.isVisible == True:
                marker.lost()

if (__name__ == '__main__'):
    print("Running unit tests for camera.py")
    camera = Camera(0, None, None, None)
    while True:
        frame = camera.videoCapture()
        cv.imshow('frame', frame)
        if cv.waitKey(1) == ord('q'):
            break
    print("Unit tests for camera.py passed")