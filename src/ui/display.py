from tkinter import *
import numpy as np

from PIL import Image, ImageTk

from . import colors as c    # NOTE this is a relative import, needed since this is the file that is run directly from the GUI

import threading

class Display():
    def __init__(self):
        self.root = Tk()
        self.root.title("Magpie Table")

        window_width = self.root.winfo_screenwidth()
        window_height = self.root.winfo_screenheight()

        self.root.geometry(f"{window_width}x{window_height}+0+0")
        self.root.attributes('-fullscreen', True)
        self.root.configure(background=c.SasakiColors.blue_4)

        self.terminate = False
    

    def register_video_label(self, dims):
        self.video_label = Label(self.root, bg=self.background)
        self.video_label.pack(side=TOP, anchor=CENTER, expand=False)

    def update_video_image(self, frame):
        image = Image.fromarray(frame)
        tkimg = ImageTk.PhotoImage(image=image)

        self.video_label.configure(image=tkimg)
        self.video_label.image = tkimg
        self.root.update()

    def build(self):
        self.add_button("X", self.end, TOP, NE, self.root)
        self.register_video_label((940, 720))
        self.add_button("Start new project", self.open_new_project_window, BOTTOM, CENTER, self.root)
        self.add_button("Print markers", self.open_new_project_window, BOTTOM, CENTER, self.root)
        
    def launch_gui(self):
        self.root.mainloop()

    def open_new_project_window(self):
        newWindow = Toplevel(self.root)
        newWindow.title("New Project")
        newWindow.geometry("800x800")
        newWindow.configure(background='black')
        # self.add_button("X", self.end(newWindow), TOP, NE, newWindow)

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

# Frame
#     - Label (video feed)
# Frame (Menu)
#     - Button
#     - Button