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
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
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
            private bool _isListening;

            public AutoUpdateWorker(GH_Component _parent) : base(_parent)
            {
                _invoker = Invoker.Instance;
                _counter = 0;
            }
            
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref _isListening);
            }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                //if (CancellationToken.IsCancellationRequested) { return; }

                _counter = 12;

                /*if (_invoker.isListening == false)
                {
                    
                    _invoker.isListening = true;
                    var task = new Task(() =>                                                          // Start a new threaded task to test if it can rerun this component
                    {
                        while (true)
                        {
                            if (CancellationToken.IsCancellationRequested) { return; }

                            _counter++;

                            // do stuff
                            ReportProgress("Progress", 0);
                            Thread.Sleep(3000);
                            // rerun the component

                        }
                    });
                }*/
            }
            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetData(0, _counter);
            }

            public override WorkerInstance Duplicate() => new AutoUpdateWorker(Parent);
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
                    GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Run", 2, 0);
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
                        MessageBox.Show("Component Run!");
                        // Run the component
                        Owner.ExpireSolution(true);
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
            get { return new Guid("510D30F6-44F6-4B3A-942C-03435A644AF1"); }
        }
    }
}