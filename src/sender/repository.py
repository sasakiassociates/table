# import repoStrategy as rs
from . import repoStrategy as rs

# import repoStrategy as rs
import threading

class Repository():
    def __init__(self, strategy_name, project_name):
        self.new_data = False
        self.strategy = rs.RepoStrategyFactory.get_strategy(strategy_name, self, project_name)
        self.project = None # TODO finalize the name (whether it's project, sketch, etc.)

    def close_threads(self):
        if self.strategy.terminate == False:
            self.strategy.terminate = True
    
    def remove_from_data(self, uuid):
        self.strategy.remove(uuid)

    # TODO update this to use the new data structure
    # bases:
    #   base*:
    #       metadata:
    #           name: base*
    #           type: base
    #       markers:
    #           marker*:
    #               id: marker*
    #               x: 0
    #               y: 0
    #               rotation: 0
    #       config:
    #           width: 0
    #           height: 0
    #       controls:
    #           control*:
    # tables:
    #   table*:
    #       current_base:
    #           base*: 0
    #       user: Scott
    #       location: fablab
    def update(self, uuid, json, gone):
        uuid = str(uuid)
        if gone:
            self.remove_from_data(uuid)
        else:
            # add marker category to data
            # TODO currently this overwrites the marker data
            json = {uuid: json}
            self.strategy.send(json)

    def push_data(self):
        self.strategy.send(self.data)
        self.new_data = False
        
if (__name__ == '__main__'):
    print("Running tests for repository.py")
    _repository = Repository('firebase')

    # setup gui using tkinter
    import tkinter as tk
    from tkinter import ttk
    from tkinter import messagebox
    from tkinter.tix import Tree
    from tkinter.ttk import Treeview

    root = tk.Tk()
    root.title("Repository Test")
    root.geometry("500x500")

    # setup gui
    def update_gui():
        if _repository.new_data:
            _repository.new_data = False
            print(_repository.data)
            Tree.delete(*Tree.get_children())
            for uuid in _repository.data["marker"]:
                Tree.insert("", "end", text=uuid, values=(_repository.data["marker"][uuid]["x"], _repository.data["marker"][uuid]["y"]))
        root.after(100, update_gui)

    _repository.strategy.receive()
    Tree = Treeview(root)
    Tree["columns"] = ("one", "two")
    Tree.column("#0", width=270, minwidth=270, stretch=tk.NO)
    Tree.column("one", width=150, minwidth=150, stretch=tk.NO)
    Tree.column("two", width=80, minwidth=50, stretch=tk.NO)

    Tree.heading("#0", text="UUID", anchor=tk.W)
    Tree.heading("one", text="Latitude", anchor=tk.W)
    Tree.heading("two", text="Longitude", anchor=tk.W)

    Tree.pack()

    root.after(100, update_gui)
    root.mainloop()
    _repository.close_threads()