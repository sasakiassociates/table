import context
import unittest

import repoStrategy

class UdpTests(unittest.TestCase):

    def test_send(self):
        strategy = repoStrategy.UDPRepo()
        strategy.setup()
        data = {}
        data[0] = {"id": 0, "location": [10, 20], "rotation": 50, "type": "model"}
        data[1] = {"id": 1, "location": [10, 0], "rotation": 30, "type": "model"}
        data[2] = {"id": 2, "location": [10, 0], "rotation": 10, "type": "model"}
        strategy.set_data(data)
        strategy.send()


if __name__ == '__main__':
    unittest.main()