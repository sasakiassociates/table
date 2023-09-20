from abc import ABC, abstractmethod

@abstractmethod
class DataStrategy(ABC):
    def __init__(self, data):
        self.data = data

    @abstractmethod
    def get_data(self):
        pass

class ProjectStrategy(DataStrategy):
    def __init__(self, data):
        super().__init__(data)

    def get_data(self):
        return self.data
    
class ModelStrategy(DataStrategy):
    def __init__(self, data):
        super().__init__(data)

    def get_data(self):
        return self.data
    
class SliderStrategy(DataStrategy):
    def __init__(self, data):
        super().__init__(data)

    def get_data(self):
        return self.data

class RotatorStrategy(DataStrategy):
    def __init__(self, data):
        super().__init__(data)

    def get_data(self):
        return self.data