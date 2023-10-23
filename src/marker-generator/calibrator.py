import cv2
from cv2 import aruco
import os
import glob

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

    def generate_filename_body(self, cap):
        self.cam_name = cap.getBackendName()
        self.cam_width = int(cap.get(3))
        self.cam_height = int(cap.get(4))
        self.filename_body = f"{self.image_directory}\\calibration_image_{self.cam_name}_{self.cam_width}x{self.cam_height}"

    def calibrate_from_images(self):
        image_files = glob.glob(os.path.join(self.image_directory, '*.png'))
        height, width = cv2.imread(image_files[0]).shape[:2]

        print("Image size:", width, "x", height)
        
        allCharucoCorners = []
        allCharucoIds = []
        allImagePoints = []
        allObjectPoints = []

        for image_file in image_files:
            image = cv2.imread(image_file)
            gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
            corners, ids, rejected = self.markerDetector.detectMarkers(gray)
            if ids is not None:
                charuco_corners, charuco_ids, marker_corners, marker_ids = self.charucoDetector.detectBoard(gray)
                aruco.drawDetectedCornersCharuco(gray, charuco_corners, charuco_ids)
                allCharucoCorners.append(charuco_corners)
                allCharucoIds.append(charuco_ids)
            while True:
                cv2.imshow('image', gray)
                key = cv2.waitKey(1) & 0xFF
                if key == ord('q'):
                    break
        
        

        aruco.calibrateCameraCharuco(allCharucoCorners, allCharucoIds, self.board, (width, height), self.matrix, self.distortion)

    def take_calibration_images(self, camera_num, n_images, interval):
        if not os.path.exists(self.image_directory):
            os.makedirs(self.image_directory)

        i = 0 # image counter
        cap = cv2.VideoCapture(camera_num)

        self.image_size = (int(cap.get(3)), int(cap.get(4)))

        self.generate_filename_body(cap)
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

            if ids is not None:
                cv2.imshow('frame', frame)
                cv2.waitKey(interval * 1000)
                cv2.imwrite(self.filename_body + str(i) + '.png', frame)
                i += 1

        cap.release()
        cv2.destroyAllWindows()


if __name__ == "__main__":
    calibrator = Calibrator()
    # calibrator.video_capture_test()
    calibrator.calibrate_from_images()
    # calibrator.take_calibration_images(1, 20, 1)