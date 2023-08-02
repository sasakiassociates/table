import cv2 as cv
import numpy as np
from cv2 import aruco

import factory

class Camera():
    def __init__(self, camera_num_, aruco_dict_, params_, repository_):
        self.camera_num = camera_num_
        self.aruco_dict_ = aruco_dict_
        self.params_ = params_
        self.repository_ = repository_

        dictionary_length = len(self.aruco_dict_.bytesList)
        self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_)
        self.detector = aruco.ArucoDetector(self.aruco_dict_, self.params_)
        
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

                cv.imshow('frame', frame_marked)
                if cv.waitKey(1) == ord('q'):
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
        self.repository_.send_data()