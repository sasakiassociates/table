import math
import uuid

from . import detectables

class Collection():
    def __init__(self, name, timer_, marker_ids) -> None:
        self.uuid = uuid.uuid4()
        self.name = name
        self.marker_ids = marker_ids
        self.observers = []
        self.type = "collection"
        self.timer = timer_
        self.is_visible = False
        self.lost_threshold = 3000     # sets the time (in milliseconds) before a marker is considered lost
        self.rigid = False

        self.visible_points = {}

    def update(self, uuid, json, object_type):
        # store the marker data
        if object_type == "marker":
            self.visible_points[uuid] = json["location"]
            
        # check if all markers are visible
        if self.check_if_all_visible():
            self.is_visible = True

    def remove(self, object_type, uuid):
        if uuid in self.visible_points:
            del self.visible_points[uuid]
    
    def get_bounds(self, points):
        min_x = min(points, key=lambda point: point[0])[0]
        min_y = min(points, key=lambda point: point[1])[1]
        max_x = max(points, key=lambda point: point[0])[0]
        max_y = max(points, key=lambda point: point[1])[1]

        return min_x, min_y, max_x, max_y
    
    def get_center(self, points):
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
    
    # TODO currently if one of the markers is lost, the zone still shows up with the marker set to (0,0)
    # need to find a way to remove the zone from the list of zones if one of the markers is lost
    def check_if_all_visible(self):
        detected_ids = self.visible_points.keys()
        if self.is_visible == True:                 # If it was already visible, run through all the markers and check if they are all visible
            for uuid in self.marker_ids:
                if uuid not in detected_ids:
                    self.timer.report_lost(self)
                    self.is_visible = False
                    return False
                else:
                    # if they are all here, report to the timer that the zone is found and notify the observers
                    self.notify_observers()
                    return True
        else:                                       # If the zone is not visible, check if all the markers are visible
            for uuid in self.marker_ids:
                if uuid not in detected_ids:
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
        points = self.visible_points.values()

        zone_data = {
            "name": self.name,
            "bounds": self.get_bounds(points),
            "center": self.get_center(points),
            "rotation": self.calculate_average_rotation(points),
            "markers": []
        }
        # Add marker ids to the json
        zone_data["markers"].append(self.marker_ids)
        return zone_data
    
    def report_lost(self):
        self.timer.report_lost(self)

    def report_found(self):
        self.timer.report_found(self)

    def lost(self):
        for observer in self.observers:
            observer.remove_from_sent_data(self.type, self.name)
        self.notify_observers()

    # TODO find a way to calculate the depth of the collection
    def calculate_depth(self, marker):
        pass