import cv2

# Create a VideoCapture object for the camera (0 is typically the default webcam)
cap = cv2.VideoCapture(1)

# Check if the camera is opened successfully
if not cap.isOpened():
    print("Error: Camera not found or could not be opened")
else:
    # Get some camera properties
    frame_width = int(cap.get(3))  # Width of the frames in the video stream
    frame_height = int(cap.get(4))  # Height of the frames in the video stream
    camera_name = cap.getBackendName()  # Get the camera backend name

    # Print camera properties
    print("Camera Name:", camera_name)
    print("Frame Width:", frame_width)
    print("Frame Height:", frame_height)

# Release the camera
cap.release()