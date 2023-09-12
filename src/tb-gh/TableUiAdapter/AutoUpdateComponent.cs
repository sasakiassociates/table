using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using GrasshopperAsyncComponent;
using Rhino.Geometry;
using TableLib;

namespace TableUiAdapter
{
    public class AutoUpdateComponent : GH_AsyncComponent
    {

        /// <summary>
        /// Initializes a new instance of the AutoUpdateComponent class.
        /// </summary>
        public AutoUpdateComponent()
          : base("AutoUpdateComponent", "Auto Update",
              "A component that opens a udp listener on another thread and when it receives a message, reruns itself",
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
            pManager.AddIntegerParameter("Test Output", "T", "Test Output", GH_ParamAccess.item);
        }

        private class AutoUpdateWorker : WorkerInstance
        {
            private Invoker _invoker;
            public int _counter;

            public AutoUpdateWorker(GH_Component _parent) : base(_parent)
            {
                _invoker = Invoker.Instance;
                _counter = _invoker._counter;
            }
            
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
            }

            public override async void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                //if (CancellationToken.IsCancellationRequested) { return; }
                var autoUpdateComponent = (AutoUpdateComponent)Parent;
                
                if (!_invoker.isListening)
                {
                    // start the listener on a separate thread
                    _ = Task.Run(() => UpdateThreadTest());
                }

                Done();
            }
            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetData(0, _counter);
            }

            public override WorkerInstance Duplicate() => new AutoUpdateWorker(Parent);

            private async Task UpdateThreadTest()
            {
                _invoker.isListening = true;
                while (_invoker.isListening)
                {
                    _invoker._counter++;

                    // Schedule a solution update on the UI thread
                    Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                    {
                        Parent.OnPingDocument().ScheduleSolution(1, (doc) =>
                        {
                            // This code will run on the UI thread
                            Parent.ExpireSolution(true);
                        });
                    }));

                    await Task.Delay(1000);
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
                        // Run the component
                        _invoker.isListening = false;
                        return GH_ObjectResponse.Handled;
                    }
                }

                return base.RespondToMouseUp(sender, e);
            }
        }

        protected override void ExpireDownStreamObjects()
        {
            base.ExpireDownStreamObjects();
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
            get { return new Guid("510D30F6-44F6-4B3A-942C-03435A644AF1"); }
        }
    }
}