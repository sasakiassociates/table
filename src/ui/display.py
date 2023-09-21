import tkinter as tk
import numpy as np

from PIL import Image, ImageTk
import os

from . import colors as c    # NOTE this is a relative import, needed since this is the file that is run directly from the GUI

import threading

class Display():
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Magpie Table")

        window_width = self.root.winfo_screenwidth()
        window_height = self.root.winfo_screenheight()

        self.root.geometry(f"{window_width}x{window_height}+0+0")
        self.root.attributes('-fullscreen', True)
        self.root.configure(background=c.SasakiColors.blue_4)

        self.button_padding = (10, 10)

        self.terminate = False
    
    def launch_gui(self):
        self.root.mainloop()


    def update_video_image(self, frame):
        image = Image.fromarray(frame)
        tkimg = ImageTk.PhotoImage(image=image)

        self.video_label.configure(image=tkimg)
        self.video_label.image = tkimg
        self.root.update()

    def build(self):
        mainFrame = tk.Frame(self.root, bg="white")

        headerFrame = tk.Frame(mainFrame)
        headerFrame.grid(row=0, column=0, columnspan=3, pady=10)

        exit_button = tk.Button(headerFrame, text="X", command=self.end)
        exit_button.grid(row=0, column=2, sticky=tk.NE)
        logo = Image.open(os.path.join(os.path.dirname(__file__), 'elements\\sasaki_logo.jpg'))
        # Set the desired width (you can change this to your desired value)
        new_height = 50

        # Calculate the new height to maintain the aspect ratio
        original_width, original_height = logo.size
        aspect_ratio = original_height / original_width
        new_width = int(new_height / aspect_ratio)

        # Resize the image while preserving the aspect ratio
        resized_logo = logo.resize((new_width, new_height))
        logo_photo = ImageTk.PhotoImage(resized_logo)
        logoLabel = tk.Label(headerFrame, image=logo_photo)
        logoLabel.image = logo_photo
        logoLabel.grid(row=0, column=0, sticky=tk.NW)

        title = tk.Label(headerFrame, text="TableUI", fg=c.SasakiColors.blue_1, font=("Helvetica", 24))
        title.grid(row=0, column=1, sticky=tk.NSEW, padx=50)

        videoFrame = tk.Frame(mainFrame)
        videoFrame.grid(row=1, column=0, sticky=tk.NSEW)
        
        self.video_label = tk.Label(videoFrame, text="Loading video feed...")
        self.video_label.grid(row=1, column=0, sticky=tk.NSEW)

        menuFrame = tk.Frame(mainFrame)
        menuFrame.grid(row=2, column=0, pady=10)
        
        tk.Button(menuFrame, text="Print markers", command=self.end).grid(row=0, column=0, sticky=tk.EW, padx=self.button_padding[0], pady=self.button_padding[1])
        tk.Button(menuFrame, text="Start new project", command=self.open_new_project_window).grid(row=0, column=1, sticky=tk.EW, padx=self.button_padding[0], pady=self.button_padding[1])
        
        mainFrame.pack(expand=True)

        self.root.update()
    
    def open_new_project_window(self):
        newWindow = tk.Toplevel(self.root)
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