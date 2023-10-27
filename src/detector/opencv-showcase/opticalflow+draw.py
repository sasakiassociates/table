import cv2
import numpy as np

# Initialize video capture from your camera or video file
cap = cv2.VideoCapture(0)  # Change the argument to your video source if needed

# Create a blank canvas to draw on
canvas = np.zeros((480, 640, 3), dtype=np.uint8)

# Parameters for Lucas-Kanade optical flow
lk_params = dict(
    winSize=(15, 15),
    maxLevel=2,
    criteria=(cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, 10, 0.03),
)

# Initialize some variables
old_frame = None
old_gray = None
points = []
colors = []

while True:
    ret, frame = cap.read()
    if not ret:
        break

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    if old_frame is not None:
        # Calculate optical flow
        p1, st, err = cv2.calcOpticalFlowPyrLK(old_gray, gray, points, None, **lk_params)

        # Filter out points with a bad status
        good_new = p1[st == 1]
        good_old = points[st == 1]

        for i, (new, old) in enumerate(zip(good_new, good_old)):
            a, b = new.ravel()
            c, d = old.ravel()
            color = (0, 0, 255)  # Red

            # Draw a line from the old point to the new point
            cv2.line(canvas, (int(a), int(b)), (int(c), int(d)), color, 2)

        points = good_new

    # Update the previous frame and points
    old_frame = frame.copy()
    old_gray = gray
    if len(points) < 10:
        mask = np.random.randint(0, 640, (10, 2))
        points = np.vstack([points, mask])
        colors.extend(np.random.randint(0, 255, (10, 3)))

    # Display the canvas with the drawings
    cv2.imshow('Optical Flow Drawing', canvas)

    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()
