# This is a possible way to reinforce detection, but doesn't seem to fix some of the issues we're having with detection
# In testing it isn't good at picking up the markers

import cv2
import cv2.aruco as aruco

# Initialize ArUco dictionary and parameters
aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_5X5_250)
parameters = aruco.DetectorParameters()
detector = aruco.ArucoDetector(aruco_dict, parameters)

# Initialize object trackers for each marker
trackers = []

# Example video capture (replace this with your video source)
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)


while True:
    # Read a frame from the video source
    ret, frame = cap.read()
    if not ret:
        break

    # Detect ArUco markers in the frame
    corners, ids, rejectedImgPoints = detector.detectMarkers(frame)

    if ids is None:
        continue
    else:
        # Iterate through detected markers
        for i in range(len(ids)):
            marker_id = ids[i][0]

            # Check if tracker for this marker is not initialized
            if len(trackers) <= i:
                # Initialize tracker for the new marker
                tracker = cv2.TrackerKCF_create()
                trackers.append(tracker)
                
                # Store initial position for the lost marker
                initial_position = corners[i][0]
                trackers[i].init(frame, (initial_position[0], initial_position[1], 
                                        initial_position[2] - initial_position[0], 
                                        initial_position[3] - initial_position[1]))

            # Update tracker for the existing markers
            else:
                success, bbox = trackers[i].update(frame)
                if success:
                    # Tracker successfully updated, use bbox for the new position
                    x, y, w, h = [int(v) for v in bbox]
                    cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
                    # Update other information or perform actions as needed

    # Display the result
    cv2.imshow('Frame', frame)

    # Break the loop if 'q' key is pressed
    if cv2.waitKey(30) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
cv2.destroyAllWindows()
