import threading
import time

class Timer(threading.Thread):
    def __init__(self):
        super().__init__()
        self.time_since_start = 0  # The current elapsed time interval
        self.running = False
        self.lock = threading.Lock()
        self.lost_objects = []
        self.time_last_sent = None
        self.send_interval = 100  # The time interval (in milliseconds) between sending data so it isn't sent too often  

    def run(self):
        self.running = True
        while self.running:
            time.sleep(0.001)  # Sleep for 1 millisecond
            with self.lock:
                self.time_since_start += 1                  # Increment the time since start
                for orphan in self.lost_objects:            # Loop through the lost items (orphans)
                    if self.time_since_start - orphan.time_last_seen > orphan.lost_threshold:
                        orphan.lost()                       # If the orphan has been lost for more than its lost_threshold, report it as lost
                        self.lost_objects.remove(orphan)    # Remove the orphan from the list of lost orphan

    def check_if_send(self):
        if self.time_last_sent is None:
            self.time_last_sent = self.time_since_start
            return True
        else:
            if self.time_since_start - self.time_last_sent > self.send_interval:
                self.time_last_sent = self.time_since_start
                return True
            else:
                return False

    def end(self):
        self.running = False

    def report_lost(self, orphan):
        with self.lock:                                     # Lock the thread so we don't have multiple threads trying to access the same data
            orphan.time_last_seen = self.time_since_start   # Set the orphan's time since seen to the current time
            self.lost_objects.append(orphan)                # Add the orphan to the list of lost orphans to keep track of
    
    def report_found(self, orphan):
        with self.lock:
            orphan.time_last_seen = None
            if orphan in self.lost_objects:
                self.lost_objects.remove(orphan)

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
