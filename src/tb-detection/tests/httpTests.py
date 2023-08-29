import context
import unittest

import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

class HttpTests(unittest.TestCase):
    def setUp(self):
        if not firebase_admin._apps:
            cred = credentials.Certificate("../key/firebase_table-key.json")
            firebase_admin.initialize_app(cred, {
                'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
            })

    def test_set(self):
        ref = db.reference('/')
        ref.set([{"id": 0, "location": [0, 0], "rotation": 0, "type": "model"}])

    def test_get(self):
        ref = db.reference('/')
        print(ref.get())


if __name__ == '__main__':
    unittest.main()