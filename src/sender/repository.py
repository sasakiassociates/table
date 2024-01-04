from . import repoStrategy as rs

import threading

class Repository():
    def __init__(self, strategy_name, project_name, event_manager):
        self.project = None # TODO finalize the name (whether it's project, sketch, etc.)

        self.message_to_push = {}
        self.new_data = False
        self.should_push_event = threading.Event()
        self.terminate = False

        self.project_name = project_name

        self.sending_thread = threading.Thread(target=self.watch_for_push, daemon=True)
        self.sending_thread.start()
        #self.listener_thread = threading.Thread(target=self.strategy.receive)

        self.strategy = rs.RepoStrategyFactory.get_strategy(strategy_name, project_name, event_manager)
        self.event_type_handler = {
            "end": self.end,
        }

        self.lock = threading.Lock()

    # Batch the messages to send
    def update(self, uuid, json):
        with self.lock:
            uuid = str(uuid)
            if json is None:
                self.strategy.remove_marker(uuid)
                if uuid in self.message_to_push:
                    del self.message_to_push[uuid]
            else:
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

            with self.lock:
                self.strategy.push(self.message_to_push)
                self.message_to_push.clear()
                self.should_push_event.clear()
                self.new_data = False
        
    def end(self):
        self.terminate = True        
        
if (__name__ == '__main__'):
    print("Running tests for repository.py")
    _repository = Repository('firebase')