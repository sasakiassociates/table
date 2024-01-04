import queue
import cv2 as cv
import numpy as np
import threading
import time

from . import marker as m

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
    def __init__(self, repository, event_manager) -> None:
        # self.ids_to_markers = {}
        self.markers = set()
        self.event_manager = event_manager
        self.repository = repository
        self.matrix = None
        # self.markers_to_destroy = set()
        self.markers_to_destroy = queue.Queue()
        
    def handle_event(self, event):
        if event["type"] == "marker_lost":
            self.markers_to_destroy.put(event["marker"])

    def make_marker(self, id_):
        new_marker = m.Marker(id_, self.event_manager)
        new_marker.attach_observer(self.repository)

        self.event_manager.attach_observer(new_marker)

        self.markers.add(new_marker)

        return new_marker

    def get_marker(self, id_):
        for marker in self.markers:
            if marker.id == id_:
                return marker
        return None
    
    def get_closest_marker(self, id_):
        marker_distances = {}
        # Go through each marker and get the distance to marker.prev_center
        # Return the marker with the smallest distance
        for marker in self.ids_to_markers[id_]:
            distance = np.linalg.norm(np.array(marker.prev_center) - np.array(marker.center))
            marker_distances[marker] = distance
        # return the marker with the smallest distance
        return min(marker_distances, key=marker_distances.get)

    def update(self, ids, corners, frame):
        self.event_manager.register_event({"type": "begin_update"})

        if ids is not None:
            for marker_id, marker_corners in zip(ids, corners):
                marker = self.get_marker(int(marker_id)) or self.make_marker(int(marker_id))
                marker.update(marker_corners, frame)

        self.event_manager.register_event({"type": "end_update", "frame": frame})

        while not self.markers_to_destroy.empty():
            marker = self.markers_to_destroy.get()
            self.event_manager.detach_observer(marker)
            self.markers.remove(marker)
            del marker

        if self.repository.new_data:
            self.repository.push()

    def end(self):
        self.event_manager.register_event({"type": "end"})

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