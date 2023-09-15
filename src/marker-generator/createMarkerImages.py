from cv2 import aruco
import cv2 as cv
import argparse

import os

parser = argparse.ArgumentParser()
parser.add_argument("--destination_folder", type=str, default="./markers/", help="Folder to save the marker images")

dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_100)
# marker = aruco.generateImageMarker(dictionary, 0, 700, None, 1)
# cv.imwrite("marker0.png", marker)

destination_folder = parser.parse_args().destination_folder
if not os.path.exists(destination_folder):
    os.makedirs(destination_folder)

for i in range(1, 100):
    marker = aruco.generateImageMarker(dictionary, i, 700, None, 1)
    cv.imwrite(destination_folder + "marker" + str(i) + ".png", marker)