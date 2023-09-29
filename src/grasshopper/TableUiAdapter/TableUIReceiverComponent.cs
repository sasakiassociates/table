﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Geometry;
using TableUiReceiver;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Drawing;
using System.Windows.Forms;

namespace TableUiAdapter
{
    public class TableUIReceiverComponent : GH_Component
    {
        List<Marker> controllerMarkers = new List<Marker>();
        List<Marker> geometryMarkers = new List<Marker>();

        public bool isListening = false;
        public bool run = true;
        bool cameraTracking = false;
        public int messageCounter = 0;

        private Repository _repository;
        private CancellationToken _cancellationToken = new CancellationToken();

        /// <summary>
        /// Initializes a new instance of the TableUIReceiver class.
        /// </summary>
        public TableUIReceiverComponent()
          : base("TableUIReceiver", "TableUI Receive",
              "A component to receive incoming data from the TableUI tangible interface system.",
              "Strategist", "TableUI")
        {
            Attributes = new TableUIReceiverAttributes(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry Markers", "Geometry", "The markers that will be assigned to geometries", GH_ParamAccess.list);
            pManager.AddGenericParameter("Controller Markers", "Controllers", "The markers that correspond to the controllers", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (run && !isListening)                
            {
                if (_repository == null)
                {
                    _repository = new Repository();
                }

                _ = Task.Run(() => ListenThread()); // Launch the listening thread
                isListening = true;
            }
            else if (!run && isListening)
            {
                _repository.UdpSend("STOP");        // "STOP" message ends the detection program and it's threads
                isListening = false;                // Setting isListening to false ends the listening thread
                controllerMarkers.Clear();
                geometryMarkers.Clear();
                messageCounter = 0;
            }

            // TODO: Build out components that use these to do something
            DA.SetDataList(0, geometryMarkers);
            DA.SetDataList(1, controllerMarkers);
        }

        private async Task ListenThread()
        {
            while (isListening)
            {
                string incomingJson = await _repository.Receive(_cancellationToken);      // Keep listening for incoming messages until we get one or the cancellation token is triggered
                List<Marker> incomingMarkers = Parser.Parse(incomingJson);                                     // Get the important values from the JSON

                List<Marker> newControllerMarkers = new List<Marker>(); 
                List<Marker> newGeometryMarkers = new List<Marker>();

                foreach (Marker marker in incomingMarkers)
                {
                    switch (marker.type)
                    {
                        case "camera":
                            // If camera tracking is on, move the camera according to this marker
                            if (cameraTracking)
                            {
                                var doc = Rhino.RhinoDoc.ActiveDoc;
                                var view = doc.Views.ActiveView;
                                var camera = view.ActiveViewport.CameraLocation;

                                Point3d cameraLocation = new Point3d(marker.location[0], marker.location[1], 5.5);
                                view.ActiveViewport.SetCameraLocation(cameraLocation, false);

                                Point3d cameraTarget = new Point3d(marker.location[0], marker.location[1]-1, 5.5);
                                cameraTarget.Transform(Transform.Rotation(marker.rotation, cameraLocation));
                                view.ActiveViewport.SetCameraTarget(cameraTarget, false);
                                view.Redraw();
                            }
                            break;
                        case "controller":
                            newControllerMarkers.Add(marker);
                            break;
                        case "geometry":
                            newGeometryMarkers.Add(marker);
                            break;
                    }
                }

                controllerMarkers = newControllerMarkers;
                geometryMarkers = newGeometryMarkers;

                // Expire the solution on the main thread (Grasshopper won't let you interact with the main thread from another thread)
                Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                {
                    ExpireSolution(true);
                }));
            }
        }

        /// <summary>
        /// These attributes allow us to add buttons to the component.
        /// TODO: add a third button below these two to trigger cancellation token
        /// </summary>
        public class TableUIReceiverAttributes : GH_ComponentAttributes
        {
            public TableUIReceiverAttributes(IGH_Component component) 
                : base(component) { }

            private Rectangle LaunchButtonBounds { get; set; }
            private Rectangle StopButtonBounds { get; set; }

            protected override void Layout()
            {
                base.Layout();
                Rectangle baseRec = GH_Convert.ToRectangle(Bounds);
                baseRec.Height += 26;
                Bounds = baseRec;

                Rectangle rec1 = GH_Convert.ToRectangle(Bounds);
                rec1.Y = rec1.Bottom - 26;
                rec1.Height = 26;
                rec1.Width = rec1.Width / 2 - 2;        // Adjust the width two buttons
                rec1.Inflate(-2, -2);
                LaunchButtonBounds = rec1;

                Rectangle rec2 = LaunchButtonBounds;
                rec2.X = rec2.Right + 4;                // Set the X position for the stop button
                StopButtonBounds = rec2;
            }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule button1 = GH_Capsule.CreateTextCapsule(LaunchButtonBounds, LaunchButtonBounds, GH_Palette.Black, "On/Off", 2, 0);
                    button1.Render(graphics, Selected, Owner.Locked, false);
                    button1.Dispose();

                    GH_Capsule button2 = GH_Capsule.CreateTextCapsule(StopButtonBounds, StopButtonBounds, GH_Palette.Black, "Track Camera On/Off", 2, 0);
                    button2.Render(graphics, Selected, Owner.Locked, false);
                    button2.Dispose();
                }
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (LaunchButtonBounds.Contains(System.Drawing.Point.Round(e.CanvasLocation)))
                    {
                        if (!((TableUIReceiverComponent)Owner).isListening)
                        {
                            ((TableUIReceiverComponent)Owner).run = true;               // Set run to true to trigger LaunchDetectionProgram in SolveInstance
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);     // Expire the solution to trigger the component to run
                        }
                        else if (((TableUIReceiverComponent)Owner).isListening)
                        {
                            ((TableUIReceiverComponent)Owner).run = false;              // Set run to false to trigger StopDetectionProgram in SolveInstance
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);     // Expire the solution to trigger the component to run
                            MessageBox.Show("The detection program is already running.");
                        }
                        return GH_ObjectResponse.Handled;
                    }
                    else if (StopButtonBounds.Contains(System.Drawing.Point.Round(e.CanvasLocation)))
                    {
                        if (!((TableUIReceiverComponent)Owner).isListening) 
                        {
                            MessageBox.Show("The detection program is not running.");
                        }
                        else if (!((TableUIReceiverComponent)Owner).cameraTracking)
                        {
                            ((TableUIReceiverComponent)Owner).cameraTracking = true;    // Start the camera tracking
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);     // Expire the solution to trigger the component to run
                        }
                        else if (((TableUIReceiverComponent)Owner).cameraTracking)
                        {
                            ((TableUIReceiverComponent)Owner).cameraTracking = false;   // Stop the camera tracking
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);     // Expire the solution to trigger the component to run
                        }
                        return GH_ObjectResponse.Handled;
                    }
                }

                return base.RespondToMouseUp(sender, e);
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            isListening = false;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("814978C9-5F8D-420E-B7C5-309E45926C0B"); }
        }
    }
}