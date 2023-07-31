using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Grasshopper.GUI.Script;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUI
{
    public class HttpGetRequestAsyncComponent : GH_Component
    {
        private string response_ = "";
        private bool shouldExpire_ = false;
        private RequestState currentState_ = RequestState.Off;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HttpGetRequestAsyncComponent()
          : base("MyComponent1", "HTTP Get",
            "An aynschronous HTTP get request sent to a Firebase realtime database",
            "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URL", "U", "URL to the Firebase database",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Trigger", "T", "Trigger the request",
                GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Expire", "E", "The length of time in ms after which the request will fail",
                GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "Response from the server",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (shouldExpire_)
            {
                switch (currentState_)
                {
                    case RequestState.Off:
                        this.message = "inactive"
                        DA.SetData(0, "");
                        currentState_ = RequestState.Idle;
                        break;
                    case RequestState.Error:
                        this.message = "ERROR"
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, response_);
                        currentState_ = RequestState.Idle;
                        break;
                    case RequestState.Done:
                        this.message = "DONE"
                        DA.SetData(0, response_);
                        currentState_ = RequestState.Idle;
                        break;
                }
                DA.SetData(0, response_);
                shouldExpire_ = false;
                return;
            }

            bool active = false;
            string url = "";
            string authToken = "";
            int timeout = 0;

            DA.GetData("Send", ref active);
            if (!active)
            {
                currentState_ = RequestState.Off;
                shouldExpire_ = true;
                ExpireSolution(true);
                return;
            }

            if (!DA.GetData("URL", ref url)) return;
            DA.GetData("Authorization", ref authToken);
            if (!DA.GetData("Expire", ref timeout)) return;

            if (url == null || url == "")
            {
                response_ = "URL is empty";
                currentState_ = RequestState.Error;
                shouldExpire_ = true;
                ExpireSolution(true);
                return;
            }

            currentState_ = RequestState.Requesting;
            this.Message = "Requesting...";

            AsyncGET(url, authToken, timeout);
        }

        protected override void ExpireDownStreamObjects()
        {
            if (shouldExpire_)
            {
                base.ExpireDownStreamObjects();
            }
        }

        private void AsyncGET(string url, string authToken, int timeout)
        {
            Task.Run () =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = timeout;

                    if (authToken != null && authToken.length > 0)
                    {
                        System.Net.ServicePointManager.Expect100Continue = true;
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                        request.PreAuthenticate = true;
                        request.Headers.Add("Authorization", authToken);
                    }
                    else
                    {
                        request.Credentials = CredentialCache.DefaultCredentials;
                    }

                    var res = request.GetResponse();
                    response_ = new StreamReader(res.GetResponseStream()).ReadToEnd();

                    currentState_ = RequestState.Done;

                    shouldExpire_ = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
                catch (Exception ex)
                {
                    response_ = ex.Message;
                    currentState_ = RequestState.Error;

                    shouldExpire_ = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
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
            get { return new Guid("8A15EB96-1C11-4DBA-8B67-7F9404745493"); }
        }
    }
}