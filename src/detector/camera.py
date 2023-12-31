import sys
import traceback

import cv2 as cv
import numpy as np
from cv2 import aruco

from . import arucoReference as ar

import os

class Camera():
    def __init__(self, camera_num, aruco_dict_name, params, board):

        aruco_dict_name = f'DICT_{aruco_dict_name}'.upper()

        if aruco_dict_name in ar.aruco_dict_mapping.keys():
            aruco_dict = aruco.getPredefinedDictionary(ar.aruco_dict_mapping[str(aruco_dict_name)])
            print("Using ArUco dictionary: ", aruco_dict_name)
        else:
            if aruco_dict_name is not None:
                print("Unknown ArUco dictionary: ", aruco_dict_name)
            aruco_dict = aruco.getPredefinedDictionary(ar.aruco_dict_mapping['DICT_6X6_100'])
            print("Using default dictionary: DICT_6X6_100")

        self.detector = aruco.ArucoDetector(aruco_dict, params)
        
        self.cap = cv.VideoCapture(camera_num, cv.CAP_DSHOW)
        self.cap.set(cv.CAP_PROP_FRAME_WIDTH, 1080)
        self.cap.set(cv.CAP_PROP_FRAME_HEIGHT, 720)

        self.matrix = None
        self.distortion = None

        self.board = board

        self.event_type_handler = {
            "end": self.end,
        }

        if not self.cap.isOpened():
            print("Cannot open camera")
            exit()

    def setup(self):
        cam_width = int(self.cap.get(3))
        cam_height = int(self.cap.get(4))

        matrix_directory = f"{os.getcwd()}\\marker_setup\\calibration_matrices"
        matrix_file = f"{matrix_directory}\\{cam_width}x{cam_height}_matrix.npy"
        distortion_file = f"{matrix_directory}\\{cam_width}x{cam_height}_distortion.npy"

        if os.path.exists(matrix_file) and os.path.exists(distortion_file):
            self.matrix = np.load(matrix_file)
            self.distortion = np.load(distortion_file)
            print("Loaded calibration matrices for camera of dimensions: ", cam_width, "x", cam_height)
        else:
            print("No calibration matrices found for camera of dimensions: ", cam_width, "x", cam_height)

            
    """
    Loop through the markers and update them
    """
    def videoCapture(self):
        try:
            ret, frame = self.cap.read()
            if ret:
                frame_gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

                if self.matrix is not None and self.distortion is not None:
                    h, w = frame_gray.shape[:2]
                    new_camera_matrix, roi = cv.getOptimalNewCameraMatrix(self.matrix, self.distortion, (w, h), 1, (w, h))

                    frame_gray = cv.undistort(frame_gray, self.matrix, self.distortion, None, self.matrix)

                # Detect the markers
                corners, ids, rejectedImgPoints = self.detector.detectMarkers(frame_gray)

                # Flip image
                frame_gray = cv.flip(frame_gray, 1)

                frame_color = cv.cvtColor(frame_gray, cv.COLOR_GRAY2BGR)

                self.board.update(ids, corners, frame_color)
                
                return frame_color
        except Exception as e:
            sys.stderr.write(str(e))
            traceback.print_exc()

    def handle_event(self, event):
        handler = self.event_type_handler.get(event["type"])
        if handler:
            handler()

    def end(self):
        self.cap.release()
        cv.destroyAllWindows()

if (__name__ == '__main__'):
    print("Running unit tests for camera.py")
    camera = Camera(0, None, None, None)
    while True:
        frame = camera.videoCapture()
        cv.imshow('frame', frame)
        if cv.waitKey(1) == ord('q'):
            break
    print("Unit tests for camera.py passed")