from abc import ABC, abstractmethod

class StateFactory:
    @staticmethod
    def make_states(board) -> None:
        states = {
            0: CalibrationState(board),
            1: PlayingState(board),
            2: PausedState(board),
            3: ClosingState(board)
        }
        return states

@abstractmethod
class BoardState(ABC):
    def __init__(self, board) -> None:
        self.board = board
        self.trigger = None

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def transition(self):
        pass

class CalibrationState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def transition(self):
        pass

class PlayingState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def transition(self):
        pass

class PausedState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def transition(self):
        pass

class ClosingState(BoardState):
    def __init__(self, board) -> None:
        super().__init__(board)

    def update(self, ids, corners):
        pass

    def draw(self, frame):
        pass

    def transition(self):
        pass