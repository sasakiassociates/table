from . import detectablesFactory as df

class Board:
    def __init__(self, observers) -> None:
        self.markers = []
        self.cells = []
        self.factory = df.detectablesFactory(observers)

    def draw(self, image, radius, fill, color):
        for marker in self.markers:
            marker.draw(image, radius, fill, color)

    '''
    Check if each marker already exists in the list of markers. If it does, update it. If it doesn't, make a new marker.
    params:
        ids: list of marker ids
        corners: list of marker corners
    '''
    def update(self, ids, corners):
        for marker_id, marker_corners in zip(ids, corners):
            marker = self.get_marker(marker_id)
            if marker is None:
                marker = self.factory.make_marker(marker_id)
                self.markers.append(marker)
            marker.update(marker_corners)

    def get_markers(self):
        return self.markers

    def get_marker(self, marker_id):
        for marker in self.markers:
            if marker.id == marker_id:
                return marker
        return None
