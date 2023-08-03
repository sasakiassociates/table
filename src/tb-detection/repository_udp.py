import socket

class Repository:
    def __init__(self, ip, port):
        self.ip = ip
        self.port = port
        self.data = {}

    def add_to_data(self, marker_id_, data):
        self.data.setdefault(str(marker_id_), data).update()

    def send_data(self):
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
                s.sendto(bytes(str([self.data]), "utf-8"), (self.ip, self.port))
                self.data = {}
        except Exception as e:
            print("Error sending data:", e)
    
    # TODO make all marker numbers part of the JSON
    # TODO change markers that haven't been updated to is_visible = False
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)