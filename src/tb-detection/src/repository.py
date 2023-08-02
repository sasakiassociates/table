import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

class Repository():
    def __init__(self, url, key_path):
        # self.credentials = credentials.Certificate("../key/magpietable-firebase-adminsdk-2sqk3-9ecfe00124.json")
        # self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
        #     'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
        # })
        self.credentials = credentials.Certificate(key_path)
        self.firebase_admin = firebase_admin.initialize_app(self.credentials, {
            'databaseURL': url
        })
        self.data = {}

    def add_to_data(self, marker_id_, data):
        self.data.setdefault(int(marker_id_), data).update()

    def send_data(self):
        try:
            ref = db.reference('/')
            new_data = ref.set([self.data])
            self.data = {}
        except Exception as e:
            print("Error sending data:", e)
    
    # TODO make all marker numbers part of the JSON
    # TODO change markers that haven't been updated to is_visible = False
    def update(self, marker_json, marker_id):
        self.add_to_data(marker_id, marker_json)