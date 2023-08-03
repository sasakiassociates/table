from cv2 import aruco
import cv2 as cv

aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)

board = aruco.CharucoBoard.create(5, 7, 0.04, 0.02, aruco_dict)
cv.imwrite('charuco.png', board)