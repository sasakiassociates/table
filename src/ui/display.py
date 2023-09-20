from tkinter import *

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

    def add_button(self, text, command):
        button = Button(self.root, text=text, command=command, font=self.h2_font, fg=self.primary_color, bg=self.background)
        button.pack( side=TOP, anchor=NE, expand=True)

    def build(self):
        exit_button = Display.add_button(self, "X", self.root.destroy)

    def launch(self):
        self.root.mainloop()
    
# Unit tests for this object that'll run when this file is run directly
if (__name__ == '__main__'):
    print("Running unit tests for display.py")
    display = Display()
    display.build()
    display.launch()
    print("Unit tests for display.py passed")