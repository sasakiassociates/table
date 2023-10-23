# ChArUco board generator
# ChArUco boards are used to calibrate the extrinsic parameters of a camera
# It basically builds a matrix of values that will account for the distortion of the camera

import cv2
from cv2 import aruco

def generate_charuco(aruco_dictionary):
    dictionary = aruco.getPredefinedDictionary(aruco_dictionary)
    board = aruco.CharucoBoard((5, 7), 1, 0.8, dictionary)
    image = board.generateImage((2000, 2000))
    cv2.imwrite("charuco.png", image)

if __name__ == "__main__":
    generate_charuco(aruco.DICT_6X6_1000)