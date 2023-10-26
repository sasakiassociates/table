from cv2 import aruco
import cv2 as cv
import argparse

import os
# from fpdf import FPDF

import marker_creator as creator

parser = argparse.ArgumentParser()
parser.add_argument("--destination_folder", type=str, default="./markers/", help="Folder to save the marker images")
parser.add_argument("--dict_size", type=int, default=100, choices=[50, 100, 250, 1000], help="Size of the dictionary to use")
parser.add_argument("--marker_bits", type=int, default=5, choices=[4, 5, 6, 7], help="Size of the marker in bits (determines the level of detail, smaller prints need more detail)")
parser.add_argument("--margin_size", type=float, default=0.5, help="Size of the margin around the marker in inches")
parser.add_argument("--marker_size", type=float, default=0.5, help="Size in inches the markers will be in the printout")

args = parser.parse_args()
marker_num = args.dict_size
marker_detail = args.marker_bits

margin_size = args.margin_size
marker_size = args.marker_size

aruco_dict_name = f'DICT_{marker_detail}X{marker_detail}_{marker_num}'
aruco_dict_name = aruco_dict_name.upper()

dict_num = getattr(aruco, aruco_dict_name)
dictionary = aruco.getPredefinedDictionary(dict_num)

images = []

destination_folder = parser.parse_args().destination_folder + "\\" + aruco_dict_name
if not os.path.exists(destination_folder):
    os.makedirs(destination_folder)

# pdf = FPDF()
# pdf.set_auto_page_break(auto=True, margin=15)
# pdf.add_page()

marker_images = []
marker_image_paths = []

for i in range(marker_num):
    quantity = int(marker_num)
    marker = aruco.generateImageMarker(dictionary, i, quantity, None, 1)
    path = destination_folder + "\\" + aruco_dict_name + "marker" + str(i) + ".png"
    cv.imwrite(path, marker)

    # png = Image.open(path)
    # marker_images.append(png)
    marker_image_paths.append(path)

_creator = creator.Creator()
_creator.create_pdf(marker_image_paths, destination_folder + "\\" + aruco_dict_name + ".pdf", marker_size, margin_size)

# Create a printable PDF of the markers
# Set the desired width (you can change this to your desired value)
pdf_path = destination_folder + "\\" + aruco_dict_name + ".pdf"

# pdf.output(pdf_path, "F")

# marker_images[0].save(pdf_path, "PDF" ,resolution=100.0, save_all=True, append_images=marker_images[1:])

# pdf.output(pdf_path, "F")