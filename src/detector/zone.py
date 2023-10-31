from . import marker

class Zone():
    def __init__(self, name) -> None:
        self.markers = []
        self.name = name
        self.observers = []

    def add_marker(self, marker_):
        self.markers.append(marker_)

    def get_markers(self):
        return self.markers
    
    def get_marker(self, marker_id):
        for marker in self.markers:
            if marker.id == marker_id:
                return marker
        return None
    
    def check_if_all_visible(self):
        for marker in self.markers:
            if marker.is_visible == False:
                return False
        return True
    
    def attach_observer(self, observer_):
        self.observers.append(observer_)

    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.build_json())

    def build_json(self):
        zone_data = {
            "name": self.name,
            "markers": []
        }
        for marker in self.markers:
            zone_data["markers"].append(marker.build_json())
        return zone_data