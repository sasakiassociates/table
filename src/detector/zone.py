from . import marker

class Zone():
    def __init__(self, name) -> None:
        self.markers = []
        self.name = name
        self.observers = []
        self.type = "zone"

    def add_marker(self, marker_):
        self.markers.append(marker_)

    def get_markers(self):
        return self.markers
    
    def get_marker(self, marker_id):
        for marker in self.markers:
            if marker.id == marker_id:
                return marker
        return None
    
    def get_points(self):
        points = []
        for marker in self.markers:
            points.append(marker.center)
        return points
    
    def get_bounds(self):
        points = []
        for marker in self.markers:
            points.append(marker.center)
        min_x = min(points, key=lambda point: point[0])[0]
        min_y = min(points, key=lambda point: point[1])[1]
        max_x = max(points, key=lambda point: point[0])[0]
        max_y = max(points, key=lambda point: point[1])[1]

        return min_x, min_y, max_x, max_y
    
    def check_if_all_visible(self):
        for marker in self.markers:
            if marker.is_visible == False:
                return False
        return True
    
    def attach_observer(self, observer_):
        self.observers.append(observer_)

    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.name, self.build_json(), self.type)

    def build_json(self):
        zone_data = {
            "bounds": self.get_bounds(),
            "markers": {}
        }
        for marker in self.markers:
            zone_data["markers"][marker.id] = marker.build_json()
        return zone_data