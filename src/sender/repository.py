from . import repoStrategy as rs

import threading

class Repository():
    def __init__(self, strategy_name, project_name):
        self.strategy = rs.RepoStrategyFactory.get_strategy(strategy_name, self, project_name)
        self.project = None # TODO finalize the name (whether it's project, sketch, etc.)

        self.message_to_push = {}
        self.new_data = False
        self.should_push_event = threading.Event()
        self.terminate = False

        self.project_name = project_name

        self.sending_thread = threading.Thread(target=self.watch_for_push, daemon=True)
        self.sending_thread.start()
        #self.listener_thread = threading.Thread(target=self.strategy.receive)

        self.event_type_handler = {
            "end": self.end,
        }
    
    # Batch the messages to send
    def update(self, uuid, json):
        uuid = str(uuid)
        self.message_to_push[uuid] = json
        self.new_data = True

    def handle_event(self, event):
        handler = self.event_type_handler.get(event["type"])
        if handler:
            handler()

    def push(self):
        self.should_push_event.set()

    # Send the message using the strategy
    def watch_for_push(self):
        while not self.terminate:
            self.should_push_event.wait()
            self.strategy.push(self.message_to_push)
            self.should_push_event.clear()
            self.message_to_push = {}
            self.new_data = False
        
    def end(self):
        self.terminate = True        
        
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