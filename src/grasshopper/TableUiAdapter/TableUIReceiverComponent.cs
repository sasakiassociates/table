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
        public List<int> markerIds = new List<int>();
        public List<float> markerRotations = new List<float>();
        public List<Point3d> markerLocations = new List<Point3d>();

        public bool isListening = false;
        public bool run = false;
        public int _counter = 0;

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
            pManager.AddIntegerParameter("Marker IDs", "IDs", "The IDs of the markers detected by the system.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Marker Rotations", "Rotations", "The rotations of the markers detected by the system.", GH_ParamAccess.list);
            pManager.AddPointParameter("Marker Locations", "Locations", "The locations of the markers detected by the system.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Counter", "Counter", "The number of times the component has been run.", GH_ParamAccess.item);
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

                Launcher.LaunchDetectionProgram();  // Launch the detection program
                _ = Task.Run(() => ListenThread()); // Launch the listening thread
                isListening = true;
            }
            else if (!run && isListening)
            {
                _repository.UdpSend("STOP");        // "STOP" message ends the detection program and it's threads
                isListening = false;                // Setting isListening to false ends the listening thread
                _counter = 0;
            }

            DA.SetDataList("Marker IDs", markerIds);
            DA.SetDataList("Marker Rotations", markerRotations);
            DA.SetDataList("Marker Locations", markerLocations);
            DA.SetData("Counter", _counter);
        }

        private async Task ListenThread()
        {
            while (isListening)
            {
                await _repository.Receive(_cancellationToken);

                _counter++;
                // Schedule a solution update on the UI thread
                Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                {
                    ExpireSolution(true);
                }));
            }
        }

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
                rec1.Width = rec1.Width / 2 - 2; // Adjust the width for two buttons
                rec1.Inflate(-2, -2);
                LaunchButtonBounds = rec1;

                Rectangle rec2 = LaunchButtonBounds;
                rec2.X = rec2.Right + 4; // Set the X position for the second button
                StopButtonBounds = rec2;
            }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule button1 = GH_Capsule.CreateTextCapsule(LaunchButtonBounds, LaunchButtonBounds, GH_Palette.Black, "Launch", 2, 0);
                    button1.Render(graphics, Selected, Owner.Locked, false);
                    button1.Dispose();

                    GH_Capsule button2 = GH_Capsule.CreateTextCapsule(StopButtonBounds, StopButtonBounds, GH_Palette.Black, "Stop", 2, 0);
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
                            ((TableUIReceiverComponent)Owner).run = true;
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);
                        }
                        else
                        {
                            MessageBox.Show("The detection program is already running.");
                        }
                        return GH_ObjectResponse.Handled;
                    }
                    else if (StopButtonBounds.Contains(System.Drawing.Point.Round(e.CanvasLocation)))
                    {
                        if (((TableUIReceiverComponent)Owner).isListening)
                        {
                            ((TableUIReceiverComponent)Owner).run = false;
                            ((TableUIReceiverComponent)Owner).ExpireSolution(true);
                        }
                        else
                        {
                            MessageBox.Show("The detection program is not running.");
                        }
                        return GH_ObjectResponse.Handled;
                    }
                }

                return base.RespondToMouseUp(sender, e);
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