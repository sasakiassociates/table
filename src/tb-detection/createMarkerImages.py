from cv2 import aruco
import cv2 as cv

import os

dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)
# marker = aruco.generateImageMarker(dictionary, 0, 700, None, 1)
# cv.imwrite("marker0.png", marker)

destination_folder = "../markers/"
if not os.path.exists(destination_folder):
    os.makedirs(destination_folder)

for i in range(1, 250):
    marker = aruco.generateImageMarker(dictionary, i, 700, None, 1)
    cv.imwrite(destination_folder + "marker" + str(i) + ".png", marker)