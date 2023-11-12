import math
import uuid

from . import marker

class Collection():
    def __init__(self, name, timer_) -> None:
        self.uuid = uuid.uuid4()
        self.markers = []
        self.name = name
        self.observers = []
        self.type = "collection"
        self.timer = timer_
        self.is_visible = False
        self.lost_threshold = 5000     # sets the time (in milliseconds) before a marker is considered lost

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
    
    def get_center(self):
        points = []
        for marker in self.markers:
            points.append(marker.center)
        min_x = min(points, key=lambda point: point[0])[0]
        min_y = min(points, key=lambda point: point[1])[1]
        max_x = max(points, key=lambda point: point[0])[0]
        max_y = max(points, key=lambda point: point[1])[1]

        return (min_x + max_x) / 2, (min_y + max_y) / 2
    
    def calculate_rotation_angle(self, point1, point2):
        dx = point2[0] - point1[0]
        dy = point2[1] - point1[1]
        return math.atan2(dy, dx)

    def calculate_average_rotation(self, points):
        total_rotation = 0
        num_pairs = len(points) - 1

        for i in range(num_pairs):
            total_rotation += self.calculate_rotation_angle(points[i], points[i + 1])

        average_rotation = total_rotation / num_pairs
        return average_rotation
    
    def check_if_all_visible(self):
        if self.is_visible == True:                 # If it is visible, run through all the markers and check if they are all visible
            for marker in self.markers:
                if marker.is_visible == False:      # If they are all not visible, report to the timer that the zone is lost and notifiy the observers
                    self.timer.report_lost(self)    # Report to the timer that the zone is lost
                    self.is_visible = False
                    self.notify_observers()
                    return False
                else:                               # If they are all visible, return True
                    self.notify_observers()
                    return True
        else:                                       # If the zone is not visible, check if all the markers are visible
            for marker in self.markers:
                if marker.is_visible == False:       # If any of them aren't visible, then it's still lost
                    return False
            # If all the markers are visible, report to the timer that the zone is found and notify the observers
            self.timer.report_found(self)
            self.is_visible = True
            self.notify_observers()
            return True
    
    def attach_observer(self, observer_):
        self.observers.append(observer_)

    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.uuid, self.build_json(), self.type)
        pass

    def build_json(self):
        zone_data = {
            "name": self.name,
            "bounds": self.get_bounds(),
            "center": self.get_center(),
            "rotation": self.calculate_average_rotation(self.get_points()),
            "markers": []
        }
        # Add marker ids to the json
        for marker in self.markers:
            zone_data["markers"].append(marker.id)
        print(zone_data)
        return zone_data
    
    def report_lost(self):
        self.timer.report_lost(self)

    def report_found(self):
        self.timer.report_found(self)

    def lost(self):
        for marker in self.markers:
            marker.lost()
        for observer in self.observers:
            observer.remove_from_sent_data(self.type, self.name)
        self.notify_observers()