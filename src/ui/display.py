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

        self.running = False
        self.terminate = False

        self.lock = threading.Lock()

    def add_button(self, text, command):
        button = Button(self.root, text=text, command=command, font=self.h2_font, fg=self.primary_color, bg=self.background)
        button.pack( side=TOP, anchor=NE, expand=True)

    def register_video_image(self, dims):
        self.canvas = Canvas(self.root, width=dims[0], height=dims[1], bg=self.background)
        self.canvas.pack(side=TOP, anchor=NW, expand=True)

    def update_video_image(self, frame):
        image = Image.fromarray(frame)
        tkimg = ImageTk.PhotoImage(image=image)

        self.lock.acquire()
        self.canvas.delete("all")
        self.canvas.create_image(0, 0, image=tkimg, anchor=NW)
        self.root.update()
        self.lock.release()

    def build(self):
        Display.add_button(self, "X", self.end)

    def launch_gui(self):
        self.running = True
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