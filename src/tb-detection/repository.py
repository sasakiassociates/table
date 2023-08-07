import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

import socket

class Repository():
    def __init__(self, url, key_path, ip, port, mode):
        # self.credentials = credentials.Certificate("../key/magpietable-firebase-adminsdk-2sqk3-9ecfe00124.json")
        # self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
        #     'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        # })
        self.credentials = credentials.Certificate(key_path)
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': url
        })
        self.data = {}
        self.mode = mode
        self.ip = ip
        self.port = port

    def add_to_data(self, marker_id_, data):
        self.data.setdefault(str(marker_id_), data).update()

    def send_data(self):
        if self.mode == "u":
            self.send_data_udp()
        elif self.mode == "f":
            self.send_data_firebase()
        elif self.mode == "b":
            self.send_data_firebase()
            self.send_data_udp()
        else:
            print("Error: mode not recognized")
        self.data = {}

    def send_data_udp(self):
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
                s.sendto(bytes(str([self.data]), "utf-8"), (self.ip, self.port))
        except Exception as e:
            print("Error sending data:", e)
    
    def send_data_firebase(self):
        try:
            ref = db.reference('/')
            ref.set([self.data])
        except Exception as e:
            print("Error sending data:", e)
    
    # TODO make all marker numbers part of the JSON
    # TODO change markers that haven't been updated to is_visible = False
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)