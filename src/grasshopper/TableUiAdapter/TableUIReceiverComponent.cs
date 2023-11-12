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
        Curve bounds;

        public bool isListening = false;
        public bool run = true;
        bool cameraTracking = false;

        private int cameraHeight = 5;
        public double scale = 1.0;

        private Repository _repository;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the TableUIReceiver class.
        /// </summary>
        public TableUIReceiverComponent()
          : base("TableUI Receiver", "TableUI Receiver",
              "A component to receive incoming data from the TableUI tangible interface system.",
              "Strategist", "TableUI")
        {
            Attributes = new TableUIReceiverAttributes(this);
            // Add an event handler for application/script exit
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                // Signal the cancellation token when the script is closing
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            };
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
            DA.GetData(1, ref topo);

            if (run && !isListening)                
            {
                if (_repository == null)
                {
                    _repository = new Repository();
                }

                _ = Task.Run(() => ListenThread(_cancellationTokenSource.Token)); // Launch the listening thread
                isListening = true;
            }
            else if (!run && isListening)
            {
                _repository.Disconnect();
                _repository = null;
                isListening = false;                // Setting isListening to false ends the listening thread
                planes.Clear();
                ids.Clear();
            }

            // TODO: Build out components that use these to do something
            DA.SetDataList("Marker IDs", ids);
            DA.SetDataList("Marker Planes", planes);
        }

        private async Task ListenThread(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)  // Keep listening until the cancellation token is triggered
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _repository.Disconnect();
                    _repository = null;
                    break;
                }

                string incomingJson = await _repository.Receive(cancellationToken);        // Keep listening for incoming messages until we get one or the cancellation token is triggered
                List<ParsableObject> incomingObjects = Parser.Parse(incomingJson);                  // Get the important values from the JSON
                
                ids = new List<int>();
                planes = new List<Plane>();

                if (incomingObjects == null) break;                                         // If there are no markers, stop listening

                foreach (ParsableObject incomingObject in incomingObjects)
                {
                    if (incomingObject is Marker marker)
                    {
                        ids.Add(marker.id);

                        if (marker.type == "marker")
                        {
                            marker.location[0] = (int)(marker.location[0] * scale);
                            marker.location[1] = (int)(marker.location[1] * scale);

                            if (topo != null)
                            {
                                Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                                Point3d topoPoint = topo.ClosestPoint(markerPoint);
                                marker.location[2] = (int)topoPoint.Z + cameraHeight;
                            }
                            else
                            {
                                marker.location[2] = 0;
                            }
                            Plane plane = new Plane(new Point3d(marker.location[0], marker.location[1], marker.location[2]), Vector3d.ZAxis);
                            plane.Rotate(marker.rotation, Vector3d.ZAxis);
                            planes.Add(plane);
                        }
                    }

                    / 

                    /*switch (marker.type)
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

*//*                            if (topo != null)
                            {
                                Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                                Point3d topoPoint = topo.ClosestPoint(markerPoint);
                                marker.location[2] = (int)topoPoint.Z + cameraHeight;
                            }
                            else
                            {
                                marker.location[2] = 0;
                            }*//*
                            Plane plane = new Plane(new Point3d(marker.location[0], marker.location[1], 0), Vector3d.ZAxis);
                            plane.Rotate(marker.rotation, Vector3d.ZAxis);
                            planes.Add(plane);
                            break;
                    }*/
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

                    GH_Capsule button2 = GH_Capsule.CreateTextCapsule(StopButtonBounds, StopButtonBounds, GH_Palette.Black, "Track Camera", 2, 0);
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
            run = false;
            if (_repository != null)
            {
                _repository.Disconnect();
                _repository = null;
            }
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