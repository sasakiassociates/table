import cv2
import numpy as np

# Initialize the webcam
cap = cv2.VideoCapture(0)

# Initialize empty list to store zone coordinates
zones = []

while True:
    ret, frame = cap.read()
    
    if not ret:
        break

    # Convert the frame to the HSV color space
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # Define the color range for your zone
    lower_color = np.array([165, 148, 143])  # Adjust these values for your specific color
    upper_color = np.array([76, 43, 30])  # Adjust these values for your specific color

    # Create a mask to identify the color within the specified range
    mask = cv2.inRange(hsv, lower_color, upper_color)

    # Find contours in the mask
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    # Iterate through the contours and draw the zones on the frame
    for contour in contours:
        if cv2.contourArea(contour) > 100:  # Adjust the area threshold as needed
            x, y, w, h = cv2.boundingRect(contour)
            cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
            zones.append((x, y, x + w, y + h))

    # Display the frame with drawn zones
    cv2.imshow("Color-Based Zones", frame)

    key = cv2.waitKey(1) & 0xFF

    # Press 'q' to exit the loop
    if key == ord('q'):
        break

# Release the webcam and close all OpenCV windows
cap.release()
cv2.destroyAllWindows()
