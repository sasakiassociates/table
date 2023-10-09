from abc import ABC, abstractmethod
import math
from detector import marker as m
import tkinter as tk

class TkViz():
    def __init__(self):
        markers = []
        self.canvas = None

    def build(self):
        self.canvas = tk.Canvas(self.root, width=1920, height=1080, background='black')

    def update_image(self, location, rotation, color, radius):
        self.canvas.create_oval(location[0] - radius, location[1] - radius, location[0] + radius, location[1] + radius, fill=color)
        self.canvas.create_line(location[0], location[1], location[0] + radius * math.cos(rotation), location[1] + radius * math.sin(rotation), fill='red')

    def launch(self):
        self.canvas.pack()
        self.root.mainloop()