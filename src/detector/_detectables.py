from abc import ABC, abstractmethod
from . import behavior2 as b
import uuid
import numpy as np

# Detectables are objects with data we want to send to those subscribed to the repository
@abstractmethod
class Detectable(ABC):
    def __init__(self, timer) -> None:
        self.uuid = uuid.uuid4()
        self.on_detection_behavior = None   # The behavior that is executed when a detectable is found
        self.observers = []
        self.type = "detectable"
        self.was_visible = False
        self.lost_threshold = 3000          # sets the time (in milliseconds) before a detectable is considered lost
        self.timer = timer                  # A reference to the timer object that allows us to report when a detectable is lost
        self.significant_change = False     # A flag that is set when a detectable has a significant change
    
    def set_open_strategy(self, strategy):
        self.behavior = strategy
    
    def attach_observer(self, observer_):
        self.observers.append(observer_)
    def notify_observers(self):
        for observer in self.observers:
            observer.update(self.uuid, self.build_json(), self.type)
    def build_json(self):
        pass

    # This is the main function that varies between detectables
    def track(self):
        pass

    # The detectable has been lost for a significant amount of time (Called by the Timer object)
    def lost(self):
        self.reset()
        self.notify_observers()
    # We just found the detectable after it was lost
    def found(self):
        self.timer.report_found(self)
        self.was_visible = True
    # We just lost tracking of the detectable
    def lost_tracking(self):
        self.timer.report_lost(self)
        self.was_visible = False
    # If this is the first time we've seen the detectable, execute the on_detection_behavior
    def first_time_seen(self):
        if self.on_detection_behavior is not None:
            self.on_detection_behavior.execute()

    # Called when the detectable is lost to reset the detectable to its default state
    def reset(self):
        pass

class Marker(Detectable):
    def __init__(self) -> None:
        super().__init__()
        self.type = "marker"
        self.location = (0,0)
        self.rotation = 0
        self.marker_id = None

    def track(self, corners_):
        rotation = self.calculate_rotation(corners_)
        center = self.calculate_center(corners_)

        # If there is a significant change in the rotation or center, notify the observers
        if self.significant_change:
            self.rotation = rotation
            self.location = center
            self.notify_observers()
        else:
            self.notify_observers()

    def calculate_center(self, corners_):
        np_center = np.mean(corners_[0], axis=0)
        center = (int(np_center[0]), int(np_center[1]))
        return center

    def calculate_rotation(self, corners_):
        corners = corners_.reshape(-1,2)
        centroid = np.mean(corners, axis=0)
        reference_vector = corners[0] - centroid
        angle_rads = np.arctan2(reference_vector[1], reference_vector[0])
        angle_degree = np.degrees(angle_rads)
        # Subtract 45 degrees and wrap it to the -180 to 180 degree range
        adjusted_angle_radians = (angle_rads - pi/4 + pi) % (2 * pi) - pi
        return adjusted_angle_radians

    def reset(self):
        self.location = (0,0)
        self.rotation = 0

class Collection(Detectable):
    def __init__(self) -> None:
        super().__init__()
        self.type = "collection"
        self.bounds = None
        self.rotation = 0
        self.components = []

    def add(self, detectable):
        self.components.append(detectable)
    def remove(self, detectable):
        self.components.remove(detectable)

    def reset(self):
        self.bounds = None
        self.rotation = 0

'''
    Cell is a detectable that represents a cell in the board. It is used to determine if a cell is occupied or not.
    The cell is considered occupied if a marker is detected within the bounds of the cell.
'''
class Cell(Detectable):
    def __init__(self) -> None:
        super().__init__()
        self.type = "cell"
        self.bounds = None
        self.occupied = False
