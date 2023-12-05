from abc import ABC, abstractmethod

@abstractmethod
class BoardState(ABC):
    def __init__(self, board) -> None:
        self.board = board
        self.trigger = None

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def end(self):
        pass

class CalibrationState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def end(self):
        pass

class PlayingState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def end(self):
        pass

class PausedState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def end(self):
        pass

class ClosingState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def end(self):
        pass