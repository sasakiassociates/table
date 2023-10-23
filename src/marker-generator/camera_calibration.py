import glob
import cv2
from cv2 import aruco
import numpy as np
import os



class Calibrator():
    def __init__(self) -> None:
        self.image_directory = f"{os.getcwd()}\\calibration_images"
        self.matrix_directory = f"{os.getcwd()}\\calibration_matrices"
        self.cam_name = None
        self.cam_width = None
        self.cam_height = None
        self.filename_body = None
        self.image_size = (1152, 768)
        

        self.dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_1000)
        self.params = aruco.DetectorParameters()
        self.board = aruco.CharucoBoard((5, 7), 1, 0.8, self.dictionary)
        self.detector = aruco.CharucoDetector(self.board)

    """
    Captures images from the camera and saves them to the specified directory.
    The images are saved as .png files with the name 'calibration_image_<i>.png',

    Parameters:
        interval: The time between each image capture in seconds.
        n_images: The number of images to capture.
        directory: The directory to save the images to.
    """
    def capture_calibration_images(self, interval, n_images, camera_num=0):
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

    def generate_filename_body(self, cap):
        self.cam_height = cap.get(4)
        self.cam_width = cap.get(3)
        self.cam_name = cap.getBackendName()
        self.filename_body = f"{self.image_directory}\\cal_image_{self.cam_name}_{self.cam_width}_{self.cam_height}_"

    """
    Calibrates the camera using the images from the calibration_images folder.
    Images are generated via the capture_calibration_images function.
    """
    def calibrate_camera_with_charuco(self, square_size):
        if not os.path.exists(self.image_directory):
            print("No calibration images found. Please capture calibration images first.")
            return

        images = os.listdir(self.image_directory)
        n_images = len(images)

        # Use glob to filter only PNG files in the directory
        png_files = glob.glob(os.path.join(self.image_directory, '*.png'))

        allCharucoCorners = []
        allCharucoIds = []

        for file in png_files:
            image = cv2.imread(file)
            gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

            detector = aruco.ArucoDetector(self.dictionary, self.params)
            currentCharucoCorners, currentCharucoIds, _ = detector.detectMarkers(gray)

            allCharucoCorners.append(currentCharucoCorners)
            allCharucoIds.append(currentCharucoIds)

        calibration, camera_matrix, dist_coeffs, rvecs, tvecs = aruco.calibrateCameraCharuco(currentCharucoCorners, currentCharucoIds, self.board, self.image_size, None, None)

        np.savez(self.matrix_directory + '/calibration.npz', calibration=calibration, camera_matrix=camera_matrix, dist_coeffs=dist_coeffs, rvecs=rvecs, tvecs=tvecs)
    
if __name__ == '__main__':
    _calibrator = Calibrator()
    # _calibrator.capture_calibration_images(1, 20, 1)
    _calibrator.calibrate_camera_with_charuco(0.8)