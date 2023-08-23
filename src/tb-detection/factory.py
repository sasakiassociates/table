import marker as m

class MarkerFactory:
    @staticmethod
    def make_markers(dict_length, observer, model_num, variable_num):
        marker_list = []
        # for i in range(dict_length):
        #     marker_list.append(marker.ModelMarker(i))
        #     marker_list[i].attach_observer(observer)
        # return marker_list
        
        # Make a marker for each model
        for i in range(model_num):
            marker_list.append(m.ModelMarker(i))
            marker_list[i].attach_observer(observer)
        # Make a marker for each variable, but assign it a unique id from the model markers
        for i in range(variable_num):
            marker_list.append(m.VariableMarker(i + model_num))
            marker_list[i + model_num].attach_observer(observer)
        # For now, we'll make the rest of the markers in the dictionary
        for i in range(dict_length - model_num - variable_num):
            marker_list.append(m.Marker(i + model_num + variable_num))
            marker_list[i + model_num + variable_num].attach_observer(observer)

        return marker_list