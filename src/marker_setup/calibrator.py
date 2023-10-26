import cv2
from cv2 import aruco
import os
import glob
import numpy as np

BOARD_SIZE = (5, 7)
SQUARE_LENGTH = 0.025
MARKER_LENGTH = 0.02

class Calibrator():
    def __init__(self) -> None:
        self.dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_1000)
        self.board = aruco.CharucoBoard(BOARD_SIZE, SQUARE_LENGTH, MARKER_LENGTH, self.dictionary)

        self.marker_params = aruco.DetectorParameters()
        self.board_params = aruco.CharucoParameters()
        self.refine_params = aruco.RefineParameters()

        self.markerDetector = aruco.ArucoDetector(self.dictionary, self.marker_params)
        self.charucoDetector = aruco.CharucoDetector(self.board, self.board_params, self.marker_params, self.refine_params)

        self.image_directory = f"{os.getcwd()}\\calibration_images"
        self.matrix_directory = f"{os.getcwd()}\\calibration_matrices"

        self.matrix = None
        self.distortion = None
        
    def video_capture_test(self):
        cap = cv2.VideoCapture(1)
        while True:
            ret, frame = cap.read()
            if not ret:
                print("Failed to capture image")
                break
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            
            corners, ids, rejected = self.markerDetector.detectMarkers(gray)
            if ids is not None:
                charuco_corners, charuco_ids, marker_corners, marker_ids = self.charucoDetector.detectBoard(gray)
                aruco.drawDetectedCornersCharuco(gray, charuco_corners, charuco_ids)
            
            cv2.imshow('frame', gray)
            if cv2.waitKey(1) == ord('q'):
                break
        cap.release()
        cv2.destroyAllWindows()

    def generate_filename_body(self, cam_num):
        cap = cv2.VideoCapture(cam_num)
        self.cam_name = cap.getBackendName()
        self.cam_width = int(cap.get(3))
        self.cam_height = int(cap.get(4))
        self.filename_body = f"{self.image_directory}\\calibration_image_{self.cam_name}_{self.cam_width}x{self.cam_height}"

    def calibrate_from_images(self, camera_num=0):
        image_files = glob.glob(os.path.join(self.image_directory, '*.png'))

        allCharucoCorners = []
        allCharucoIds = []
        allImagePoints = []
        allObjectPoints = []

        self.generate_filename_body(camera_num)

        for image_file in image_files:
            # Check if the camera name in the file matches the current camera name
            if f"{self.cam_width}x{self.cam_height}" not in image_file:
                continue

            height, width = cv2.imread(image_files[0]).shape[:2]

            image = cv2.imread(image_file)
            gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
            current_object_points = self.board.getObjPoints()
            current_image_points = []
            # corners, ids, rejected = self.markerDetector.detectMarkers(gray)
            charuco_corners, charuco_ids, marker_corners, marker_ids = self.charucoDetector.detectBoard(gray)
            if charuco_corners is not None and charuco_ids is not None and len(charuco_corners) > 4:
                
                aruco.drawDetectedCornersCharuco(gray, charuco_corners, charuco_ids)
                
                allCharucoCorners.append(charuco_corners)
                allCharucoIds.append(charuco_ids)

                current_object_points, current_image_points = self.board.matchImagePoints(charuco_corners, charuco_ids)

                allObjectPoints.append(current_object_points)
                allImagePoints.append(current_image_points)
            else:
                print("Not enough charuco corners found in image")
                continue

            while True:
                cv2.imshow('image', gray)
                key = cv2.waitKey(1) & 0xFF
                if key == ord('q'):
                    break
        
        retval, self.matrix, self.distortion, rvecs, tvecs = cv2.calibrateCamera(allObjectPoints, allImagePoints, (width, height), None, None)

        # aruco.calibrateCameraCharuco(allCharucoCorners, allCharucoIds, self.board, (width, height), self.matrix, self.distortion)

        print("Camera Matrix:")
        print(self.matrix)
        print("Distortion Coefficients:")
        print(self.distortion)

        if not os.path.exists(self.matrix_directory):
            os.makedirs(self.matrix_directory)

        np.save(f"{self.matrix_directory}\\{self.cam_width}x{self.cam_height}_matrix.npy", self.matrix)
        np.save(f"{self.matrix_directory}\\{self.cam_width}x{self.cam_height}_distortion.npy", self.distortion)

    def take_calibration_images(self, camera_num, n_images, interval):
        if not os.path.exists(self.image_directory):
            os.makedirs(self.image_directory)

        i = 0 # image counter
        cap = cv2.VideoCapture(camera_num)

        self.image_size = (int(cap.get(3)), int(cap.get(4)))

        self.generate_filename_body(camera_num)
        print("Capturing for Camera Name:", self.cam_name)

        if not cap.isOpened():
            print("Cannot open camera")
            exit()
        params = aruco.DetectorParameters()
        marker_detector = aruco.ArucoDetector(aruco.getPredefinedDictionary(aruco.DICT_6X6_1000), params)

        while i < n_images:
            ret, frame = cap.read()
            if not ret:
                print("Failed to capture image")
                break
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            corners, ids, rejected = marker_detector.detectMarkers(gray)

            if ids is not None and len(ids) > 4:
                cv2.imshow('frame', frame)
                cv2.waitKey(interval * 1000)
                cv2.imwrite(self.filename_body + str(i) + '.png', frame)
                i += 1

        cap.release()
        cv2.destroyAllWindows()


if __name__ == "__main__":
    calibrator = Calibrator()
    # calibrator.video_capture_test()
    # calibrator.take_calibration_images(0, 20, 1)
    calibrator.calibrate_from_images(camera_num=1)