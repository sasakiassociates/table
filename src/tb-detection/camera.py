import cv2 as cv
import numpy as np
from cv2 import aruco

import factory

class Camera():
    def __init__(self, camera_num, aruco_dict, params, repository_):
        self.camera_num = camera_num
        self.aruco_dict = aruco_dict
        self.params = params
        self.repository = repository_
        self.model_num = repository_.model_num
        self.variable_num = repository_.variable_num

        dictionary_length = len(self.aruco_dict.bytesList)
        self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_, self.model_num, self.variable_num)
        self.detector = aruco.ArucoDetector(self.aruco_dict, self.params)
        
        self.cap = cv.VideoCapture(self.camera_num, cv.CAP_DSHOW)
        self.cap.set(cv.CAP_PROP_FRAME_WIDTH, 1080)
        self.cap.set(cv.CAP_PROP_FRAME_HEIGHT, 720)

    def video_loop(self):
        if not self.cap.isOpened():
            print("Cannot open camera")
            exit()
        while True:
            ret, frame = self.cap.read()
            if ret:
                frame_gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

                # Detect the markers
                corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                frame_marked = aruco.drawDetectedMarkers(frame_gray, corners, ids)

                if ids is not None:
                    self.markerLoop(ids, corners)
                self.repository.update_send_data()

                cv.imshow('frame', frame_marked)
                if cv.waitKey(1) == ord('q') or self.repository.check_for_terminate():
                    break
        self.cap.release()
        cv.destroyAllWindows()

    '''
    Loop through the markers and update them
    
    @param ids: the ids of the markers
    @param corners: the corners of the markers
    '''
    def markerLoop(self, ids, corners):
        for marker_id, marker_corners in zip(ids, corners):
            marker_id = marker_id[0]
            marker = self.my_markers[marker_id]
            marker.update(marker_corners)