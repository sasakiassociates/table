class EventManager:
    def __init__(self):
        self.observers = []

    def attach_observer(self, observer_):
        self.observers.append(observer_)

    def detach_observer(self, observer_):
        self.observers.remove(observer_)

    def register_event(self, event):
        for observer in self.observers:
            observer.handle_event(event)