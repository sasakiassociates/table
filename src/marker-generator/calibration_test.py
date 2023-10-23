import glob
import os
import numpy as np
import cv2
from cv2 import aruco

# Define the size of the Charuco board (in squares and the square size in meters)
# Adjust these values based on the actual dimensions of your Charuco board.
board_width = 5  # Number of squares in the width
board_height = 7  # Number of squares in the height
square_size = 0.025  # Size of each square in meters

# Create a Charuco board
dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_1000)
board = aruco.CharucoBoard(
    size=(board_width, board_height),
    squareLength=square_size,
    markerLength=0.02,
    dictionary=dictionary
)

# Create lists to store object points and image points from calibration images
obj_points = []  # 3D points in real world space
img_points = []  # 2D points in image plane

# Set termination criteria for the calibration process
criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 100, 0.0001)

# Capture images for calibration (you can replace these with your own images)
image_path = f"{os.getcwd()}\\calibration_images"

image_paths = glob.glob(os.path.join(image_path, '*.png'))

marker_params = aruco.DetectorParameters()
detector = aruco.ArucoDetector(dictionary, marker_params)
charuco_params = aruco.CharucoParameters()
refine_params = aruco.RefineParameters()

# VideoCapture cycle
while True:
    cap = cv2.VideoCapture(0)
    ret, frame = cap.read()
    if not ret:
        print("Failed to capture image")
        break

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    # Detect markers and Charuco corners in the image
    corners, ids, _ = detector.detectMarkers(gray)

    print(len(corners))

    if len(corners) > 0:
        # Estimate the pose of the Charuco board
        
        boardDetector = aruco.CharucoDetector(board, charuco_params, marker_params, refine_params)

        charuco_corners, charuco_ids, marker_corners, marker_ids = boardDetector.detectBoard(gray)
        print(charuco_corners)
        # aruco.drawDetectedMarkers(gray, corners, ids)
        aruco.drawDetectedCornersCharuco(gray, charuco_corners, charuco_ids)

    cv2.imshow('image', gray)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# for image_path in image_paths:
#     image = cv2.imread(image_path)
#     gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

#     # Detect markers and Charuco corners in the image
#     corners, ids, _ = detector.detectMarkers(gray)

#     print(len(corners))

#     if len(corners) > 0:
#         aruco.drawDetectedMarkers(image, corners, ids)

#         while True:
#             cv2.imshow('image', image)
#             key = cv2.waitKey(1) & 0xFF
#             if key == ord('q'):
#                 break
        
#         # Estimate the pose of the Charuco board
#         charuco_params = aruco.CharucoParameters()
#         refine_params = aruco.RefineParameters()
#         boardDetector = aruco.CharucoDetector(board, charuco_params, marker_params, refine_params)
#         charuco_corners, charuco_ids, marker_corners, marker_ids = boardDetector.detectBoard(gray)
#         # retval, charuco_corners, charuco_ids = aruco.interpolateCornersCharuco(
#         #     corners, ids, gray, board
#         # )

#         print(charuco_corners)

#         if len(charuco_ids) > 0:
#             aruco.drawDetectedCornersCharuco(image, charuco_corners, charuco_ids)

#             # Append object points and image points for this image
#             objp = np.zeros((len(charuco_ids), 3), np.float32)
#             objp[:, :2] = board.chessboardCorners

#             img_points.append(charuco_corners)
#             obj_points.append(objp)

# # Perform camera calibration
# ret, camera_matrix, dist_coeffs, rvecs, tvecs = cv2.calibrateCamera(
#     obj_points, img_points, gray.shape[::-1], None, None
# )

# # Print calibration results
# print("Camera matrix:\n", camera_matrix)
# print("Distortion coefficients:\n", dist_coeffs)

# # Save calibration results to a file
# np.savez('camera_calibration.npz', camera_matrix=camera_matrix, dist_coeffs=dist_coeffs)

# # Undistort an example image using the calibration parameters
# image = cv2.imread('example_image.jpg')
# undistorted_image = cv2.undistort(image, camera_matrix, dist_coeffs)
# cv2.imwrite('undistorted_image.jpg', undistorted_image)