import cv2

# Create a KCF tracker
tracker = cv2.TrackerKCF.create()

# Example video capture (replace this with your video source)
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

# Read the first frame from the video
ret, frame = cap.read()
if not ret:
    print("Error reading video")
    exit()

# Select a region of interest (ROI) to track
bbox = cv2.selectROI("Frame", frame, fromCenter=False, showCrosshair=True)
tracker.init(frame, bbox)

while True:
    # Read a frame from the video source
    ret, frame = cap.read()
    if not ret:
        break

    # Update the tracker
    success, bbox = tracker.update(frame)

    # Draw the bounding box on the frame
    if success:
        x, y, w, h = [int(v) for v in bbox]
        cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)

    # Display the result
    cv2.imshow('Frame', frame)

    # Break the loop if 'q' key is pressed
    if cv2.waitKey(30) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
cv2.destroyAllWindows()
