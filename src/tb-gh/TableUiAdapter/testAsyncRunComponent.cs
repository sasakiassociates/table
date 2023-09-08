using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiAdapter
{
    public class testAsyncRunComponent : GH_Component
    {
        private UdpClient udpClient;
        private int udpPort = 5005; // Set your desired UDP port number

        /// <summary>
        /// Initializes a new instance of the testAsyncRunComponent class.
        /// </summary>
        public testAsyncRunComponent()
          : base("testAsyncRunComponent", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
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
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (udpClient == null)
            {
                udpClient = new UdpClient(udpPort);
                udpClient.BeginReceive(ReceiveCallback, null);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, udpPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
            string receivedMessage = Encoding.ASCII.GetString(receivedBytes);

            // Trigger the component to expire and recompute when a message is received
            ExpireSolution(true);

            // Continue listening for more UDP messages
            udpClient.BeginReceive(ReceiveCallback, null);
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
            get { return new Guid("4FB9CD70-A03B-41B7-B642-2E76D635AEF4"); }
        }
    }
}