import threading
import time

class Timer(threading.Thread):
    def __init__(self):
        super().__init__()
        self.time_since_start = 0  # The current elapsed time interval
        self.running = False
        self.lock = threading.Lock()
        self.lost_markers = []

    def run(self):
        self.running = True
        while self.running:
            time.sleep(0.001)  # Sleep for 1 millisecond
            with self.lock:
                self.time_since_start += 1                  # Increment the time since start
                for marker in self.lost_markers:            # Loop through the lost markers
                    if self.time_since_start - marker.time_last_seen > marker.lost_threshold:
                        marker.lost()                       # If the marker has been lost for more than its lost_threshold, report it as lost
                        self.lost_markers.remove(marker)    # Remove the marker from the list of lost markers

    def stop(self):
        self.running = False

    def report_lost(self, marker):
        with self.lock:                                     # Lock the thread so we don't have multiple threads trying to access the same data
            marker.time_last_seen = self.time_since_start   # Set the marker's time since seen to the current time
            self.lost_markers.append(marker)                # Add the marker to the list of lost markers to keep track of
    
    def report_found(self, marker):
        with self.lock:
            marker.time_last_seen = None
            if marker in self.lost_markers:
                self.lost_markers.remove(marker)

if __name__ == "__main__":
    # Create a Timer thread
    timer = Timer()
    timer.start()

    try:
        while True:
            time.sleep(2)  # Sleep for 2 seconds between checks

    except KeyboardInterrupt:
        timer.stop()
        timer.join()
