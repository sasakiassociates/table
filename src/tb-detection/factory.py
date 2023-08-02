import marker

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length_, observer_):
        marker_list = []
        for i in range(dict_length_):
            marker_list.append(marker.ModelMarker(i))
            marker_list[i].attach_observer(observer_)
        return marker_list