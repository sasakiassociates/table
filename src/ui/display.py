from tkinter import *
import numpy as np

from PIL import Image, ImageTk

import threading

class Display():
    def __init__(self):
        self.root = Tk()
        self.root.title("Magpie Table")

        window_width = self.root.winfo_screenwidth()
        window_height = self.root.winfo_screenheight()
        self.root.geometry(f"{window_width}x{window_height}+0+0")
        self.root.attributes('-fullscreen', True)
        self.root.configure(background='black')

        self.background = "black"
        self.primary_color = "white"
        self.h1_font = ("Arial", 20)
        self.h2_font = ("Arial", 16)
        self.p_font = ("Arial", 12)

        self.video_label = None
        self.canvas = None

        self.terminate = False

    def add_button(self, text, command):
        button = Button(self.root, text=text, command=command, font=self.h2_font, fg=self.primary_color, bg=self.background)
        button.pack( side=TOP, anchor=NE, expand=True)

    def register_video_label(self, dims):
        self.video_label = Label(self.root, bg=self.background, width=dims[0], height=dims[1])
        self.video_label.pack(side=TOP, anchor=NW, expand=True)

    def update_video_image(self, frame):
        image = Image.fromarray(frame)
        tkimg = ImageTk.PhotoImage(image=image)

        self.video_label.configure(image=tkimg)
        self.video_label.image = tkimg
        self.root.update()

    def build(self):
        Display.add_button(self, "X", self.end)

    def launch_gui(self):
        self.root.mainloop()

    def end(self):
        self.terminate = True
        self.root.quit()

# Unit tests for this object that'll run when this file is run directly
if (__name__ == '__main__'):
    print("Running unit tests for display.py")
    display = Display()
    display.build()
    display.launch()
    print("Unit tests for display.py passed")