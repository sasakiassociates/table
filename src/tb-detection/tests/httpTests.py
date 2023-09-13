import context
import unittest

import repoStrategy

import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

class HttpTests(unittest.TestCase):
    # def setUp(self):
    #     if not firebase_admin._apps:
    #         cred = credentials.Certificate("../key/firebase_table-key.json")
    #         firebase_admin.initialize_app(cred, {
    #             'databaseURL': 'https://magpietable-default-rtdb.firebaseio.com/'
    #         })

    # def test_set(self):
    #     ref = db.reference('/')
    #     ref.set([{"id": 0, "location": [10, 0], "rotation": 0, "type": "model"}])

    # def test_get(self):
    #     ref = db.reference('/')
    #     print(ref.get())

    def test_send(self):
        strategy = repoStrategy.UDPRepo()
        strategy.setup()
        data = {}
        data[0] = {"id": 0, "location": [10, 0], "rotation": 0, "type": "model"}
        data[1] = {"id": 1, "location": [10, 0], "rotation": 0, "type": "model"}
        strategy.set_data(data)
        strategy.send()


if __name__ == '__main__':
    unittest.main()