import cv2 as cv
from . import cell
from . import markerFactory as factory
from . import detectables as m

class Board():
    def __init__(self) -> None:
        self.my_markers, self.bounding_zone = None, None
        self.cells = []
        self.width = 0
        self.height = 0

        self.radius = 10
        self.fill = -1
        self.color = (255, 255, 255)

    def setup(self, dictionary_length, repository_, timer_, camera_resolution, camera_num, cell_num=10):
        width = camera_resolution[0]
        height = camera_resolution[1]

        # get the smaller dimension
        small_dim = min(width, height)
        large_dim = max(width, height)

        if small_dim == 0 or large_dim == 0:
            print("ERROR: Camera resolution is 0")
            return

        # get the size of each cell
        cell_size = small_dim // cell_num

        # get the number of cells that will fit in the larger dimension
        large_num_cells = large_dim // cell_size

        # calculate the starting position to center the grid
        start_x = (width - large_num_cells * cell_size) // 2
        start_y = (height - cell_num * cell_size) // 2
        
        observers = []
        
        for i in range(large_num_cells):
            for j in range(cell_num):
                id_ = j * large_num_cells + i
                new_cell = cell.Cell(id_ , start_x + i * cell_size, start_y + j * cell_size, cell_size)
                self.add_cell(new_cell)
                # observers.append(new_cell)
        
        observers.append(repository_)

        self.my_markers, self.bounding_zone = factory.MarkerFactory.make_markers(dictionary_length, observers, timer_, self.cells)

    def set_draw_params(self, radius, fill, color):
        self.radius = radius
        self.fill = fill
        self.color = color

    def add_marker(self, marker):
        self.markers.append(marker)

    def add_cell(self, cell):
        self.cells.append(cell)

    def scan_cells(self):
        for cell in self.cells:
            cell.occupied = False
        for marker in self.my_markers:
            if marker.is_visible & marker.housed == False:
                for cell in self.cells:
                    if marker.is_in_area(cell.area):
                        cell.add(marker)
                        break
        # if self.bounding_zone.is_visible:
        #     bounds = self.bounding_zone.bounds
        #     for cell in self.cells:
        #         if cell.area[0] > bounds[0] and cell.area[1] > bounds[1] and cell.area[2] < bounds[2] and cell.area[3] < bounds[3]:
        #             cell.occupied = True
        if self.bounding_zone is not None:
            bounds = self.bounding_zone.get_bounds()
            bounds = (bounds[0]-cell.size, bounds[1]-cell.size, bounds[2]+cell.size, bounds[3]+cell.size)
            for cell in self.cells:
                if cell.area[0] > bounds[0] and cell.area[1] > bounds[1] and cell.area[2] < bounds[2] and cell.area[3] < bounds[3]:
                    cell.occupied = True              

    def marker_loop(self, frame, ids, corners):
        if ids is not None:
            for marker_id, marker_corners in zip(ids, corners):
                marker = self.my_markers[int(marker_id)]

                if isinstance(marker, m.ProjectMarker):
                    if marker.running == False:
                        marker.open_project()
                else:
                    if marker.is_visible == False:
                        marker.found()
                        marker.track(marker_corners)
                        marker.flip_center(frame.shape[1])
                    else:
                        marker.track(marker_corners)
                        marker.flip_center(frame.shape[1])
                    
                    x = marker.center[0]
                    y = marker.center[1]
                    cv.ellipse(frame, (int(x), int(y)), (self.radius, self.radius), 0, 0, 360, self.color, self.fill)
                    cv.putText(frame, str(marker.id), (int(x+self.radius*1.25), int(y+self.radius/2)), cv.FONT_HERSHEY_SIMPLEX, 0.5, self.color, 1, cv.LINE_AA)
            for marker in self.my_markers:
                if marker.is_visible == True and marker.id not in ids:
                    marker.lost_tracking()
                    marker.is_visible = False
        else:
            for marker in self.my_markers:
                if marker.is_visible == True:
                    marker.lost_tracking()
                    marker.is_visible = False

        self.scan_cells()
        self.draw_board(frame)

        return frame

    def draw_board(self, frame):
        overlay = frame.copy()
        for cell in self.cells:
            cell.draw_cell(overlay)
        cv.addWeighted(overlay, 0.75, frame, 0.75, 0, frame)