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

    # Compute the magnitude of optical flow vectors
    magnitude = np.sqrt(flow[..., 0] ** 2 + flow[..., 1] ** 2)

    # Create a binary mask to segment moving objects
    threshold = 2.0  # Adjust this threshold as needed
    moving_mask = (magnitude > threshold).astype(np.uint8) * 255

    # Apply morphological operations to clean the mask
    kernel = np.ones((5, 5), np.uint8)
    moving_mask = cv2.morphologyEx(moving_mask, cv2.MORPH_OPEN, kernel)
    moving_mask = cv2.morphologyEx(moving_mask, cv2.MORPH_CLOSE, kernel)

    # Apply the mask to the original frame to show only moving objects
    moving_objects = cv2.bitwise_and(frame2, frame2, mask=moving_mask)

    # Display the frame with moving objects
    cv2.imshow("Moving Objects", moving_objects)

    # Update the previous frame
    prev_frame = next_frame

    # Exit the loop when the 'q' key is pressed
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the video capture and close all windows
cap.release()
cv2.destroyAllWindows()
