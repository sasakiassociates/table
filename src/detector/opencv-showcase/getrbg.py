import cv2

# Function to handle mouse events
def get_rgb_value(event, x, y, flags, param):
    if event == cv2.EVENT_LBUTTONDOWN:
        # Get the RGB value at the clicked pixel
        b, g, r = frame[y, x]
        print(f"Clicked pixel at (x, y): ({x}, {y}) has RGB value: ({r}, {g}, {b})")

# Initialize the webcam
cap = cv2.VideoCapture(0)

# Create a named window for displaying the webcam feed
cv2.namedWindow("Live Webcam Feed")

# Set the mouse callback function
cv2.setMouseCallback("Live Webcam Feed", get_rgb_value)

while True:
    ret, frame = cap.read()
    
    if not ret:
        break

    # Display the live webcam feed
    cv2.imshow("Live Webcam Feed", frame)

    key = cv2.waitKey(1) & 0xFF

    # Press 'q' to exit the loop
    if key == ord('q'):
        break

# Release the webcam and close all OpenCV windows
cap.release()
cv2.destroyAllWindows()
