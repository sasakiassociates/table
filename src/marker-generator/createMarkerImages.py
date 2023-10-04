from cv2 import aruco
import cv2 as cv
import argparse

import os
from PIL import Image
from fpdf import FPDF

parser = argparse.ArgumentParser()
parser.add_argument("--destination_folder", type=str, default="./markers/", help="Folder to save the marker images")
parser.add_argument("--dictionary_size", type=int, default=100, choices=[50, 100, 250, 1000], help="Size of the dictionary to use")

args = parser.parse_args()
marker_num = args.dictionary_size

if marker_num == 50:
    dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_50)
elif marker_num == 100:
    dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_100)
elif marker_num == 250:
    dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)
elif marker_num == 1000:
    dictionary = aruco.getPredefinedDictionary(aruco.DICT_6X6_1000)

images = []

destination_folder = parser.parse_args().destination_folder
if not os.path.exists(destination_folder + f"{marker_num}/"):
    os.makedirs(destination_folder + f"{marker_num}/")

pdf = FPDF()
pdf.set_auto_page_break(auto=True, margin=15)
pdf.add_page()

for i in range(0, marker_num):
    marker = aruco.generateImageMarker(dictionary, i, 100, None, 1)
    path = destination_folder + f"{marker_num}/" + "marker" + str(i) + ".png"
    cv.imwrite(path, marker)
    png = Image.open(path)
    pdf.image(path, x=15, y=15, w=10)

# Create a printable PDF of the markers
# Set the desired width (you can change this to your desired value)
pdf_path = destination_folder + f"{marker_num}/" + "markers.pdf"

pdf.output(pdf_path, "F")