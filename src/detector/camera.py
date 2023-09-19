import cv2 as cv
import numpy as np
from cv2 import aruco

import sys
import traceback

import markerFactory as factory

class Camera():
    def __init__(self, camera_num, aruco_dict, params, repository_):
        self.repository = repository_

        dictionary_length = len(aruco_dict.bytesList)
        self.my_markers = factory.MarkerFactory.make_markers(dictionary_length, repository_)
        self.detector = aruco.ArucoDetector(aruco_dict, params)
        
        self.cap = cv.VideoCapture(camera_num, cv.CAP_DSHOW)
        self.cap.set(cv.CAP_PROP_FRAME_WIDTH, 1080)
        self.cap.set(cv.CAP_PROP_FRAME_HEIGHT, 720)

        self.changed_data = False

    def videoLoop(self):
        if not self.cap.isOpened():
            print("Cannot open camera")
            exit()
        while True:
            try:
                ret, frame = self.cap.read()
                if ret:
                    frame_gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

                    # Detect the markers
                    corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                    frame_marked = aruco.drawDetectedMarkers(frame_gray, corners, ids)

                    # Update the markers
                    for marker in self.my_markers:
                        if marker.id in ids & marker.isVisible == False:
                            marker.found()
                        elif marker.id in ids & marker.isVisible == True:
                            marker.tracking()
                        elif marker.id not in ids & marker.isVisible == True:
                            marker.lost()
                            
                    if self.changed_data:
                        self.repository.send_data()
                        self.changed_data = False

                    cv.imshow('frame', frame_marked)
                    if cv.waitKey(1) == ord('q') or self.repository.check_for_terminate():
                        break
            except Exception as e:
                sys.stderr.write(str(e))
                traceback.print_exc()
                break
        self.cap.release()
        cv.destroyAllWindows()
        # self.repository.close_threads()

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
            if marker.significant_change:
                self.changed_data = True