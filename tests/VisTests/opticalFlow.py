import cv2
import numpy as np

# Open the video capture
cap = cv2.VideoCapture(0)

# Create an initial empty frame
ret, frame1 = cap.read()
prev_frame = cv2.cvtColor(frame1, cv2.COLOR_BGR2GRAY)

while True:
    # Read the next frame
    ret, frame2 = cap.read()
    if not ret:
        break

    # Convert the frame to grayscale
    next_frame = cv2.cvtColor(frame2, cv2.COLOR_BGR2GRAY)

    # Compute optical flow
    flow = cv2.calcOpticalFlowFarneback(prev_frame, next_frame, None, 0.5, 3, 15, 3, 5, 1.2, 0)

    # Create a grid of points for visualization
    h, w = flow.shape[:2]
    y, x = np.mgrid[0:h, 0:w]
    fx, fy = flow[..., 0], flow[..., 1]

    # Draw the flow vectors on the frame
    step = 25  # Spacing between vectors
    for i in range(0, h, step):
        for j in range(0, w, step):
            cv2.line(frame2, (j, i), (j + int(fx[i, j]), i + int(fy[i, j])), (0, 255, 0), 2)
    
    # Display the frame with optical flow
    cv2.imshow("Optical Flow", frame2)

    # Update the previous frame
    prev_frame = next_frame

    # Exit the loop when the 'q' key is pressed
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the video capture and close all windows
cap.release()
cv2.destroyAllWindows()
