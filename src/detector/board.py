import cv2 as cv
import numpy as np
import threading

from . import marker as m
from . import timer as t
from . import state as s

class BoardSingletonMeta(type):
    _instances = {}
    _lock = threading.Lock()

    def __call__(cls, *args, **kwargs):
        with cls._lock:
            if cls not in cls._instances:
                instance = super().__call__(*args, **kwargs)
                cls._instances[cls] = instance
            return cls._instances[cls]

class Board(metaclass=BoardSingletonMeta):
    def __init__(self, repository) -> None:
        self.markers = {}       # The markers currently present on the board with their ids as the keys
        self.bounds = None      # Holds the (x, y) coordinates of the calibration markers from setup
        self.timer = t.Timer()
        self.timer.start()
        self.repository = repository
        self.state = None
        self.trigger_state_pairs = s.StateFactory.make_states(self)
        self.markers_to_delete = []
        self.id_occurrences = {}

    def make_marker(self, id_):
        new_marker = m.Marker(id_, self.timer)
        new_marker.attach_observer(self.repository)

        # If the marker id is not in the dictionary, add it
        if id_ not in self.markers.keys():
            self.markers[id_] = []
        # Add the marker to the list of markers with that id
        self.markers[id_].append(new_marker)

        return new_marker
    
    def destroy_markers(self):
        if len(self.markers_to_delete) > 0:
            print(self.markers_to_delete)
            print("Destroying markers")
            for marker in self.markers_to_delete:
                self.markers[marker.id].remove(marker)
                self.id_occurrences[marker.id] -= 1
                if self.id_occurrences[marker.id] == 0:
                    del self.id_occurrences[marker.id]
                if len(self.markers[marker.id]) == 0:
                    del self.markers[marker.id]
            self.markers_to_delete = []

    def get_marker(self, id_):
        for marker in self.markers[id_]:
            # If the marker was already updated, skip it
            if marker.updated:
                continue
            # If it hasn't been updated, update it and return it
            elif not marker.updated:
                return marker
        # If no markers are found, return None
        return None

    # Called by the Camera object
    # Runs through the detected ids and checks if they are already on the board
    # If they are, check if there's been a significant change to that marker
    # If there has, update the marker
    # If not, make a new marker object with a uuid and add it to the board
    # TODO expect there to be multiple markers with the same id
    def update(self, ids, corners):
        # Mark each marker as not updated
        for id_, markers in self.markers.items():
            for marker in markers:
                marker.updated = False
        
        if ids is not None:

            for marker_id, marker_corners in zip(ids, corners):
                id_ = int(marker_id)
                if id_ not in self.markers.keys():
                    # 1. Marker does not exist and has been detected
                    marker = self.make_marker(id_)
                    marker.update(marker_corners)
                    self.id_occurrences[id_] = 1
                else:
                    # 2. Marker exists and has been detected
                    # If the id does not exist in the self.id_occurrences dictionary, we haven't updated this marker yet
                    # So we update it
                    # if id_ not in self.id_occurrences.keys():
                    #     marker = self.make_marker(id_)
                    #     marker.update(marker_corners)
                    #     self.id_occurrences[id_] = 1
                    # If the id does exist in the self.id_occurrences dictionary, that means there are multiple markers in there (or should be)
                    # So if there is another marker in there we update it, otherwise we make a new marker
                    # else:
                    # TODO figure out how to handle it when there are multiple markers with the same id and one disappears (currently it moves to where the other one is)
                        marker = self.get_marker(id_)
                        if marker is not None:
                            if marker.is_visible:
                                marker.update(marker_corners)
                        else:
                            marker = self.make_marker(id_)
                            marker.update(marker_corners)
                            self.id_occurrences[id_] += 1

        # 3. Marker exists and has not been detected
        for id_, markers in self.markers.items():
            for marker in markers:
                if marker.updated:
                    continue
                elif marker.gone:
                    self.markers_to_delete.append(marker)
                elif marker.is_visible:
                    print("Lost tracking of marker: ", marker.id)
                    marker.lost_tracking()
        self.destroy_markers()

        if self.repository.new_data:
            self.repository.strategy.send()
            self.repository.new_data = False


    def draw(self, frame):
        # Draw the markers
        for id_, markers in self.markers.items():
            for marker in markers:
                marker.draw(frame)

    def end(self):
        self.timer.end()

    def change_state(self, state):
        self.state = state

    # TODO implement a calibration phase that runs these calculations when it's done
    # TODO implement the perspective transform to get the bounds of the board
    def find_transformation_matrix(pixel_points, cad_points):
        """
        Find the transformation matrix from pixel space to CAD space.

        Args:
        pixel_points (list of tuples): Points in pixel space [(x1, y1), (x2, y2), (x3, y3), (x4, y4)]
        cad_points (list of tuples): Corresponding points in CAD space [(x1, y1), (x2, y2), (x3, y3), (x4, y4)]

        Returns:
        numpy.ndarray: Transformation matrix from pixel space to CAD space
        """
        # Convert points to numpy arrays
        pixel_array = np.float32(pixel_points)
        cad_array = np.float32(cad_points)

        # Calculate the transformation matrix
        matrix = cv.getPerspectiveTransform(pixel_array, cad_array)

        return matrix

    # TODO make this happen in the individual markers
    def apply_transformation(matrix, point):
        """
        Apply the transformation matrix to a point.

        Args:
        matrix (numpy.ndarray): Transformation matrix
        point (tuple): Point in pixel space (x, y)

        Returns:
        tuple: Transformed point in CAD space
        """
        point_array = np.float32([[point]])
        transformed_point = cv.perspectiveTransform(point_array, matrix)

        return transformed_point[0][0]