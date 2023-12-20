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
        self.repository = repository
        self.markers_to_delete = []
        self.matrix = None
        
        self.timer.start()

    def make_marker(self, id_):
        new_marker = m.Marker(id_, self.timer)
        new_marker.attach_observer(self.repository)

        # If the marker id is not in the dictionary, add it
        if id_ not in self.markers.keys():
            self.markers[id_] = []
        # Add the marker to the list of markers with that id
        self.markers[id_].append(new_marker)

        # if self.id_occurrences.get(id_) is None:
        #     self.id_occurrences[id_] = 1
        # else:
        #     self.id_occurrences[id_] += 1

        return new_marker
    
    def destroy_markers(self):
        if len(self.markers_to_delete) > 0:
            for marker in self.markers_to_delete:
                self.markers[marker.id].remove(marker)
                # self.id_occurrences[marker.id] -= 1
                # if self.id_occurrences[marker.id] == 0:
                #     del self.id_occurrences[marker.id]
                if len(self.markers[marker.id]) == 0:
                    del self.markers[marker.id]
            self.markers_to_delete = []

    def get_marker(self, id_):
        for marker in self.markers[id_]:
            # If the marker was already updated, skip it
            if marker.updated:
                continue
            # If it hasn't been updated, make sure it's the closest one to the previous center, and return it
            elif not marker.updated:
                self.get_closest_marker(id_)
                return marker
        # If no markers are found, return None
        return None
    
    def get_closest_marker(self, id_):
        marker_distances = {}
        # Go through each marker and get the distance to marker.prev_center
        # Return the marker with the smallest distance
        for marker in self.markers[id_]:
            distance = np.linalg.norm(np.array(marker.prev_center) - np.array(marker.center))
            marker_distances[marker] = distance
        # return the marker with the smallest distance
        return min(marker_distances, key=marker_distances.get)

    # Called by the Camera object
    # Runs through the detected ids and checks if they are already on the board
    # If they are, check if there's been a significant change to that marker
    # If there has, update the marker
    # If not, make a new marker object with a uuid and add it to the board
    # TODO expect there to be multiple markers with the same id
    # TODO there's an issue here
    #       The markers aren't deleted before they run through and update again, this makes it so all markers get deleted but then it cycles through all markers again and makes zombie markers reaappear on the database
    def update(self, ids, corners):
        id_occurences = {}
        updated_id_occurrences = {}

        # Mark each marker as not updated
        for id_, markers in self.markers.items():
            for marker in markers:
                marker.updated = False

        # reorder markers in each id so visible markers are first
        for id_, markers in self.markers.items():
            visible_markers = []
            invisible_markers = []
            for marker in markers:
                if marker.is_visible:
                    visible_markers.append(marker)
                else:
                    invisible_markers.append(marker)
            self.markers[id_] = visible_markers + invisible_markers
        
        if ids is not None:

            # Get the number of occurrences of each id
            for id_ in ids:
                id_ = int(id_)
                if id_ not in id_occurences.keys():
                    id_occurences[id_] = 1
                else:
                    id_occurences[id_] += 1

            for marker_id, marker_corners in zip(ids, corners):
                id_ = int(marker_id)
                if id_ not in self.markers.keys():
                    # 1. Marker does not exist and has been detected
                    # Create Key and Marker then increment self.id_occurrences
                    marker = self.make_marker(id_)
                    marker.update(marker_corners)
                    updated_id_occurrences[id_] = 1
                else:
                    # 2. Marker exists and has been detected
                    
                    # If we're in here, the marker id exists and has at least one detected marker inside it
                    # Get a marker that has not been updated yet
                    marker = self.get_marker(id_)

                    # If there is a marker that hasn't been updated yet
                    # TODO This mostly works but still does the updates in an arbitrary order. We need to somehow make sure "get marker" returns a marker in the closer vicinity of the previous marker
                    if marker is not None:
                        # If the number of updated markers is less than the number of detected markers
                        if updated_id_occurrences.get(id_) is None or updated_id_occurrences[id_] < id_occurences[id_]:
                            # Check if the marker is visible
                            if marker.is_visible:
                                marker.update(marker_corners)
                                if updated_id_occurrences.get(id_) is None:
                                    updated_id_occurrences[id_] = 1
                                else:
                                    updated_id_occurrences[id_] += 1
                            else:
                                marker.found()
                                marker.update(marker_corners)
                                if updated_id_occurrences.get(id_) is None:
                                    updated_id_occurrences[id_] = 1
                                else:
                                    updated_id_occurrences[id_] += 1
                        # If the number of updated markers is equal to the number of detected markers, then we've already updated all the markers we need to
                        elif updated_id_occurrences[id_] == id_occurences[id_]:
                            continue
                    else:
                        # If all markers under this id have already been updated, make a new marker
                        marker = self.make_marker(id_)
                        marker.update(marker_corners)
                        updated_id_occurrences[id_] += 1

        # 3. Marker exists and has not been detected
        for id_, markers in self.markers.items():
            for marker in markers:
                if marker.updated:
                    continue
                elif marker.gone:
                    self.markers_to_delete.append(marker)
                elif marker.is_visible:
                    marker.lost_tracking()
        self.destroy_markers()

    def draw(self, frame):
        # Draw the markers
        for id_, markers in self.markers.items():
            for marker in markers:
                marker.draw(frame)

    def end(self):
        self.timer.end()

    # TODO implement a calibration phase that runs these calculations when it's done
    # TODO implement the perspective transform to get the bounds of the board
    def find_transformation_matrix(self, pixel_points, cad_points):
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
        self.matrix = cv.getPerspectiveTransform(pixel_array, cad_array)

    # TODO make this happen in the individual markers
    def apply_transformation(self, point):
        """
        Apply the transformation matrix to a point.

        Args:
        matrix (numpy.ndarray): Transformation matrix
        point (tuple): Point in pixel space (x, y)

        Returns:
        tuple: Transformed point in CAD space
        """
        if self.matrix == None:
            raise Exception("No transformation matrix found")

        point_array = np.float32([[point]])
        transformed_point = cv.perspectiveTransform(point_array, self.matrix)

        return transformed_point[0][0]