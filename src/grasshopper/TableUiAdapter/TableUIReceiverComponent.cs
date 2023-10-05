using System;
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
using Grasshopper.GUI.SettingsControls;

namespace TableUiAdapter
{
    public class TableUIReceiverComponent : GH_Component
    {
        List<int> ids = new List<int>();
        List<Plane> planes = new List<Plane>();
        Mesh topo = new Mesh();

        public bool isListening = false;
        public bool run = true;
        bool cameraTracking = false;

        public double scale = 1.0;

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
            pManager.AddNumberParameter("Scale", "S", "Adjust the scale of changes the markers affect", GH_ParamAccess.item, 1.0);
            pManager.AddMeshParameter("Topography", "T", "(Optional) The topography of the base model", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Marker IDs", "IDs", "The IDs of all currently detected markers", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Marker Planes", "Planes", "The planes that represent the location of the markers", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref scale);

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
                planes.Clear();
                ids.Clear();
            }

            // TODO: Build out components that use these to do something
            DA.SetDataList("Marker IDs", ids);
            DA.SetDataList("Transform Planes", planes);
        }

        private async Task ListenThread()
        {
            while (isListening)
            {
                string incomingJson = await _repository.Receive(_cancellationToken);        // Keep listening for incoming messages until we get one or the cancellation token is triggered
                List<Marker> incomingMarkers = Parser.Parse(incomingJson);                  // Get the important values from the JSON

                ids.Clear();
                planes.Clear();

                foreach (Marker marker in incomingMarkers)
                {
                    ids.Add(marker.id);

                    switch (marker.type)
                    {
                        case "camera":
                            if (cameraTracking)
                            {
                                var doc = Rhino.RhinoDoc.ActiveDoc;
                                var view = doc.Views.ActiveView;
                                var camera = view.ActiveViewport.CameraLocation;

                                double z = 5.5;
                                Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                                if (topo != null)
                                {
                                    Point3d closestPoint = topo.ClosestPoint(markerPoint);
                                    z = closestPoint.Z + 5.5;
                                }

                                Point3d cameraLocation = new Point3d(marker.location[0], marker.location[1], z);
                                view.ActiveViewport.SetCameraLocation(cameraLocation, false);

                                Point3d cameraTarget = new Point3d(marker.location[0] - 1, marker.location[1], z);
                                Transform rotation = Transform.Rotation(marker.rotation, cameraLocation);
                                cameraTarget.Transform(rotation);

                                Vector3d cameraDirection = cameraTarget - cameraLocation;

                                view.ActiveViewport.SetCameraDirection(cameraDirection, false);

                                view.ActiveViewport.SetCameraTarget(cameraTarget, false);
                                view.Redraw();
                            }
                            break;
                        case "marker":
                            marker.location[0] = (int)(marker.location[0] * scale);
                            marker.location[1] = (int)(marker.location[1] * scale);
                            if (topo != null)
                            {
                                Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                                Point3d topoPoint = topo.ClosestPoint(markerPoint);
                                marker.location[2] = (int)topoPoint.Z;
                            }
                            Plane plane = new Plane(new Point3d(marker.location[0], marker.location[1], marker.location[2]), new Vector3d(marker.rotation, 0, 0));
                            planes.Add(plane);
                            break;
                    }
                }

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
                            
                            var doc = Rhino.RhinoDoc.ActiveDoc;
                            var view = doc.Views.ActiveView;
                            var camera = view.ActiveViewport.CameraLocation;
                            Point3d cameraLocation = new Point3d(100, 100, 0);
                            view.ActiveViewport.SetCameraLocation(cameraLocation, false);
                            Point3d cameraTarget = new Point3d(0, 0, 0);
                            view.ActiveViewport.SetCameraTarget(cameraTarget, false);
                            view.Redraw();

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