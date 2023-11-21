from . import timer as t
from . import detectables as d
from . import zone as z

class detectablesFactory:
    def __init__(self, observers) -> None:
        self.timer = t.Timer()
        self.timer.start()
        self.observers = observers # The observers that want to be updated on Detectables changes

    def make_marker(self, id_):
        marker = d.Marker(id_, self.timer)
        marker.attach_observer(self.observers)
        return marker

    def make_zone(self, ids_):
        zone = z.Zone(ids_, self.timer)
        zone.attach_observer(self.observers)
        return zone