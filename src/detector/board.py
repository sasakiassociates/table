from . import marker as m
from . import timer as t

class Board():
    def __init__(self, repository) -> None:
        self.markers = {}       # The markers currently present on the board with their ids as the keys
        self.bounds = None      # Holds the (x, y) coordinates of the calibration markers from setup
        self.timer = t.Timer()
        self.timer.start()
        self.repository = repository

    def make_marker(self, id_, center):
        new_marker = m.Marker(id_, self.timer)
        new_marker.center = center

        self.markers[id_] = new_marker
    
    def destroy_marker(self, uuid):
        for marker in self.markers:
            if marker.uuid == uuid:
                self.markers.remove(marker)

    def get_marker(self, id_):
        for marker in self.markers:
            if marker.id == id_:
                return marker
        return None

    # Called by the Camera object
    # Runs through the detected ids and checks if they are already on the board
    # If they are, check if there's been a significant change to that marker
    # If there has, update the marker
    # If not, make a new marker object with a uuid and add it to the board
    # TODO expect there to be multiple markers with the same id
    def update(self, ids, corners):
        # We want to go through all the markers on the board and see if they've been detected
        # TODO have to find a way to check which ids/corners go with which marker objects

        if ids is not None:
            for marker_id, marker_corners in zip(ids, corners):
                marker = self.get_marker(marker_id)
                # 1. Marker exists and has been detected
                if marker is not None:
                    marker.update(marker_corners)
                # 2. Marker does not exist and has been detected
                else:
                    self.make_marker(marker_id, marker_corners)
        # 3. Marker exists and has not been detected
        for marker in self.markers:
            if marker.id not in ids:
                if marker.is_visible:
                    marker.lost_tracking()
                elif marker.lost:
                    self.destroy_marker(marker.uuid)
        
        if self.repository.new_data:
            self.repository.strategy.send()
            self.repository.new_data = False

    def end(self):
        self.timer.end()