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
        self.board = aruco.CharucoBoard((5, 7), 1, 0.5, self.dictionary)
        self.detector = aruco.CharucoDetector(self.board)
        self.markerDetector = aruco.ArucoDetector(self.dictionary, self.params)

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

    def live_board_detection(self):
        cap = cv2.VideoCapture(0)
        while True:
            ret, frame = cap.read()
            if not ret:
                print("Failed to capture image")
                break
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            corners, ids, rejected = self.markerDetector.detectMarkers(gray)
            # aruco.drawDetectedMarkers(gray, corners, ids)

            charucoCorners = None
            charucoIds = None
            
            self.detector.detectBoard(gray, charucoCorners, charucoIds, corners, ids)

            print(charucoCorners)

            if charucoCorners is not None and charucoIds is not None:
                aruco.drawDetectedCornersCharuco(gray, charucoCorners, charucoIds)

                for corner in charucoCorners:
                    cv2.ellipse(gray, (int(corner[0][0]), int(corner[0][1])), (5, 5), 0, 0, 360, (0, 0, 255), 2)

                # Append object points and image points for this image
                objp = np.zeros((len(charucoIds), 3), np.float32)
                objp[:, :2] = self.board.chessboardCorners

            cv2.imshow('gray', gray)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break


    """
    Calibrates the camera using the images from the calibration_images folder.
    Images are generated via the capture_calibration_images function.
    """
    def calibrate_camera_with_charuco(self, square_size):
        if not os.path.exists(self.image_directory):
            print("No calibration images found. Please capture calibration images first.")
            return

        # Use glob to filter only PNG files in the directory
        png_files = glob.glob(os.path.join(self.image_directory, '*.png'))

        allCharucoCorners = []
        allCharucoIds = []

        for file in png_files:
            print(image)
            image = cv2.imread(file)
            gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

            corners, ids, _ = self.markerDetector.detectMarkers(gray)

            self.detector.detectBoard(gray, currentCharucoCorners, currentCharucoIds, corners, ids)

            aruco.drawDetectedCornersCharuco(gray, currentCharucoCorners, currentCharucoIds)


            detector = aruco.ArucoDetector(self.dictionary, self.params)
            currentCharucoCorners, currentCharucoIds, _ = detector.detectMarkers(gray)

            allCharucoCorners.append(currentCharucoCorners)
            allCharucoIds.append(currentCharucoIds)

        camera_matrix = None
        dist_coeffs = None
        rvecs = None
        tvecs = None
        aruco.calibrateCameraCharuco(currentCharucoCorners, currentCharucoIds, self.board, self.image_size, camera_matrix, dist_coeffs, rvecs, tvecs)

        if camera_matrix is not None and dist_coeffs is not None:
            calibration = np.array([camera_matrix, dist_coeffs])
        np.savez(self.matrix_directory + '/calibration.npz', calibration=calibration, camera_matrix=camera_matrix, dist_coeffs=dist_coeffs, rvecs=rvecs, tvecs=tvecs)
    
if __name__ == '__main__':
    _calibrator = Calibrator()
    # _calibrator.capture_calibration_images(1, 20, 1)
    # _calibrator.calibrate_camera_with_charuco(0.8)
    _calibrator.live_board_detection()