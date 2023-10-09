from abc import ABC, abstractmethod
from src import marker as m
import tkinter as tk

@abstractmethod
class Visualizer(ABC):
    def __init__(self):
        markers = []

    def setup(self):
        self.canvas = tk.Canvas(self.root, width=1920, height=1080, background='black')

    def update_image(self, frame):
        pass