import cv2

class Cell:
    def __init__(self, id_, x, y, size) -> None:
        self.id = id_
        self.x = x
        self.y = y
        self.data = {}
        self.markers = []
        self.size = size
        self.occupied = False
        self.area = (self.x, self.y, self.x + self.size, self.y + self.size)

    def get_id(self):
        return self.id
    
    def draw_cell(self, frame):
        if self.occupied:
            cv2.rectangle(frame, (self.x, self.y), (self.x + self.size, self.y + self.size), (255, 255, 255), -1)        
        else:
            # Write the id of the cell in the middle, center justified
            # text = str(self.id)
            # text_size = cv2.getTextSize(text, cv2.FONT_HERSHEY_SIMPLEX, 1, 2)
            # text_x = int(self.x + (self.size - text_size[0][0]) / 2)
            # text_y = int(self.y + (self.size + text_size[0][1]) / 2)
            # cv2.putText(frame, text, (text_x, text_y), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2, cv2.LINE_AA)
            cv2.rectangle(frame, (self.x, self.y), (self.x + self.size, self.y + self.size), (255, 255, 255), 1)

    def add(self, marker):
        self.markers.append(marker)
        self.occupied = True

    def remove(self, marker):
        self.markers.remove(marker)
        if len(self.markers) == 0:
            self.occupied = False

    def check_if_new_data(self):
        pass