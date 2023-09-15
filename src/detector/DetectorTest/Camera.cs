using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Aruco;
using Emgu.CV.Linemod;
using Emgu.CV.Structure;

class Camera
{
    public static MCvScalar borderColor = new MCvScalar(0, 255, 0);

    static void Main(string[] args)
    {
        // Initialize video capture from the default camera (you can change the index if needed)
        VideoCapture capture = new VideoCapture(0);

        if (!capture.IsOpened)
        {
            Console.WriteLine("Error: Unable to access the camera.");
            return;
        }

        WindowFlags windowFlags = WindowFlags.AutoSize | WindowFlags.FreeRatio | WindowFlags.KeepRatio;
        // Create a window to display the camera feed
        CvInvoke.NamedWindow("Camera Feed", windowFlags);

        // Create an ArUco dictionary (6x6_100 in this case)
        Dictionary dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_100);

        while (true)
        {
            // Read a frame from the camera
            Mat frame = capture.QueryFrame();

            if (frame == null)
                break;

            // Detect ArUco markers in the frame
            VectorOfInt markerIds = new VectorOfInt();
            VectorOfVectorOfPointF markerCorners = new VectorOfVectorOfPointF();
            VectorOfVectorOfPointF rejected = new VectorOfVectorOfPointF();
            DetectorParameters parameters = DetectorParameters.GetDefault();
            ArucoInvoke.DetectMarkers(frame, dictionary, markerCorners, markerIds, parameters, rejected);

            // Draw detected markers on the frame
            if (markerIds.Size > 0)
            {
                ArucoInvoke.DrawDetectedMarkers(frame, markerCorners, markerIds, borderColor);
            }

            // Display the frame
            CvInvoke.Imshow("Camera Feed", frame);

            // Break the loop if the 'Esc' key is pressed
            if (CvInvoke.WaitKey(1) == 27) // 27 is the ASCII code for the 'Esc' key
                break;
        }

        // Release the camera and destroy the window when done
        capture.Dispose();
        //capture.Release();
        CvInvoke.DestroyAllWindows();
    }
}
