import marker as m

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer):
        marker_list = []
        for i in range(dict_length):
            marker_list.append(m.Marker(i))
            marker_list[i].attach_observer(observer)
            
            if i == dict_length - 1:
                marker_list[i].set_type("camera")
            elif i >= dict_length - 5:
                marker_list[i].set_type("rotator")
            elif i >= dict_length - 9:
                marker_list[i].set_type("slider")
            else:
                marker_list[i].set_type("model")

        return marker_list