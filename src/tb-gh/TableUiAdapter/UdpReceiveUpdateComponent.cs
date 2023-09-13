using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using GrasshopperAsyncComponent;
using Rhino.Geometry;
using TableLib;
using static TableUiAdapter.TestAutoUpdateComponent;
using System.Drawing;

namespace TableUiAdapter
{
    public class UdpReceiveUpdateComponent : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the UdpAutoUpdatingComponent class.
        /// </summary>
        public UdpReceiveUpdateComponent()
          : base("UdpReceive (Auto-Update)", "UdpReceive",
              "A component that launches a separate thread that listens for UDP messages that will re-run the component whenever it gets a new message",
              "Strategist", "TableUI")
        {
            BaseWorker = new AutoUpdateWorker(this);
            Attributes = new AutoUpdateAttributes(this);
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
            pManager.AddIntegerParameter("Marker Ids", "M", "Marker Ids", GH_ParamAccess.list);
        }

        private class AutoUpdateWorker : WorkerInstance
        {
            // Makes singleton "Invoker", whos object "Repository" connects to a UDP Client to listen on port 5005
            // Needs to be a singleton across threads for this method to work
            private Invoker _invoker;
            public List<Marker> markers;
            private bool newMarkers = false;
            public List<int> markerids = new List<int>();

            public AutoUpdateWorker(GH_Component _parent) : base(_parent)
            {
                _invoker = Invoker.Instance;
            }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
            }
            
            /// <summary>
            /// This method will continue to complain about not waiting for the thread, but there are controls so it's fine
            /// </summary>
            /// <param name="ReportProgress"></param>
            /// <param name="Done"></param>
            public override async void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                if (CancellationToken.IsCancellationRequested) { return; }

                if (!_invoker.isListening)
                {
                    // start the listener on a separate thread
                    _ = Task.Run(() => UpdateThreadTest());
                }

                if (newMarkers)
                {
                    foreach (Marker marker in markers)
                    {
                        markerids.Add(marker.id);
                    }
                }

                Done();
            }
            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetDataList(0, markerids);
            }

            public override WorkerInstance Duplicate() => new AutoUpdateWorker(Parent);

            /// <summary>
            /// This method will run on a separate thread and cause the component to expire if it receives a udp message on port 5005
            /// </summary>
            /// <returns></returns>
            private async Task UpdateThreadTest()
            {
                _invoker.isListening = true;
                while (_invoker.isListening)
                {
                    // wait until there's a message from the udp client
                    List<Marker> updatedMarkers = await _invoker.ListenerThread(CancellationToken);

                    // If there's an update to the markers, expire the component to run it again
                    if (updatedMarkers != null)
                    {
                        // update the markers
                        markers = updatedMarkers;
                        newMarkers = true;

                        // Schedule a solution update on the UI thread
                        Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                        {
                            Parent.OnPingDocument().ScheduleSolution(1, (doc) =>
                            {
                                // This code will run on the UI thread
                                Parent.ExpireSolution(true);
                            });
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// This adds a menu item to the component that allows the user to cancel the asynchronous operation.
        /// </summary>
        /// <param name="menu"></param>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel", (s, e) =>
            {
                RequestCancellation();
            });
        }

        public class AutoUpdateAttributes : GH_ComponentAttributes
        {
            private Invoker _invoker = Invoker.Instance;
            public AutoUpdateAttributes(GH_Component owner)
                : base(owner) { }
            private Rectangle ButtonBounds { get; set; }

            protected override void Layout()
            {
                base.Layout();
                Rectangle baseRec = GH_Convert.ToRectangle(Bounds);
                baseRec.Height += 26;
                Bounds = baseRec;

                Rectangle rec = GH_Convert.ToRectangle(Bounds);
                rec.Y = rec.Bottom - 26;
                rec.Height = 26;
                rec.Inflate(-2, -2);
                ButtonBounds = rec;
            }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "End", 2, 0);
                    button.Render(graphics, Selected, Owner.Locked, false);
                    button.Dispose();
                }
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (ButtonBounds.Contains(System.Drawing.Point.Round(e.CanvasLocation)))
                    {
                        // React to the button click here.
                        // You can trigger your desired action when the button is clicked.
                        // For example, you can show a message box.
                        MessageBox.Show("Listening Stopped!");
                        _invoker.isListening = false;

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
            get { return new Guid("EFBFBA52-EE65-44B1-B8C4-7977585F8BE7"); }
        }
    }
}