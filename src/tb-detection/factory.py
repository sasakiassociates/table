import marker as m

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer):
        marker_list = []
        for i in range(dict_length):
            marker_list.append(m.Marker(i-1))
            marker_list[i].attach_observer(observer)
        return marker_list