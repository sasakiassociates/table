import cv2 as cv
import numpy as np

from . import marker as m
from . import timer as t

class Board():
    def __init__(self, repository) -> None:
        self.markers = {}       # The markers currently present on the board with their ids as the keys
        self.bounds = None      # Holds the (x, y) coordinates of the calibration markers from setup
        self.timer = t.Timer()
        self.timer.start()
        self.repository = repository

    def make_marker(self, id_, corners):
        new_marker = m.Marker(id_, self.timer)
        new_marker.attach_observer(self.repository)
        new_marker.update(corners)

        self.markers[id_] = new_marker
    
    def destroy_marker(self, uuid):
        for marker in self.markers.values():
            if marker.uuid == uuid:
                del self.markers[marker.id]
                return

    def get_marker(self, id_):
        for marker in self.markers.values():
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
                id_ = int(marker_id)
                marker = self.get_marker(id_)
                # 1. Marker exists and has been detected
                if marker is not None:
                    marker.update(marker_corners)
                # 2. Marker does not exist and has been detected
                else:
                    self.make_marker(id_, marker_corners)
            # 3. Marker exists and has not been detected
            for marker_id, marker in list(self.markers.items()):
                if marker_id not in ids:
                    if marker.is_visible:
                        marker.lost_tracking()
                    elif marker.gone:
                        self.destroy_marker(marker.uuid)
        else:
            for marker_id, marker in list(self.markers.items()):
                if marker.is_visible:
                    marker.lost_tracking()
                elif marker.gone:
                    self.destroy_marker(marker.uuid)
        
        if self.repository.new_data:
            self.repository.strategy.send()
            self.repository.new_data = False

    def draw(self, frame):
        for marker in self.markers.values():
            marker.draw(frame)

    def end(self):
        self.timer.end()

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