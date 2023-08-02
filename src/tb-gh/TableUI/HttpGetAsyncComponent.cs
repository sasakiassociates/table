using System;
using System.Collections.Generic;
using System.Net;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.Render;

namespace TableUI
{
    public class HttpGetAsyncComponent : GH_Component
    {
        private string _response = "";
        private bool _shouldExpire = false;
        private RunState _currentState = RunState.Off;

        
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HttpGetAsyncComponent()
          : base("Http Get Async", "Http Get",
              "Runs an Http get request to the given url",
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
            pManager.AddBooleanParameter("Send", "S", "Send the request",
                               GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Expire", "E", "The length of time in ms after which the request will fail",
                               GH_ParamAccess.item, 1000);
            pManager.AddTextParameter("Authorization", "A", "The authentication ID for the database",
                               GH_ParamAccess.item, "");
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
            if (_shouldExpire)
            {
                switch (_currentState)
                {
                    case RunState.Off:
                        this.Message = "inactive";
                                DA.SetData(0, "");
                        _currentState = RunState.Idle;
                        break;
                    case RunState.Error:
                        this.Message = "ERROR";
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _response);
                        _currentState = RunState.Idle;
                        break;
                    case RunState.Done:
                        this.Message = "done";
                                DA.SetData(0, _response);
                        _currentState = RunState.Idle;
                        break;
                }
                DA.SetData(0, _response);
                _shouldExpire = false;
                return;
            }

            bool active = false;
            string url = "";
            string authToken = "";
            int timeout = 0;

            DA.GetData("Send", ref active);
            if (!active)
            {
                _currentState = RunState.Off;
                _shouldExpire = true;
                ExpireSolution(true);
                return;
            }

            if (!DA.GetData("URL", ref url)) return;
            DA.GetData("Authorization", ref authToken);
            if (!DA.GetData("Expire", ref timeout)) return;

            if (url == null || url.Length == 0)
            {
                _response = "URL is empty";
                _currentState = RunState.Error;
                _shouldExpire = true;
                ExpireSolution(true);
                return;
            }

            _currentState = RunState.Running;
            this.Message = "Requesting...";

            AsyncGet(url, authToken, timeout);
        }

        protected override void ExpireDownStreamObjects()
        {
            if (_shouldExpire)
            {
                base.ExpireDownStreamObjects();
            }
        }

        private void AsyncGet(string url, string authToken, int timeout)
        {
            Task.Run(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = timeout;

                    if (authToken != null && authToken.Length > 0)
                    {
                        System.Net.ServicePointManager.Expect100Continue = true;
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        request.PreAuthenticate = true;
                        request.Headers.Add("Authorization", authToken);
                    }
                    else
                    {
                        request.Credentials = CredentialCache.DefaultCredentials;
                    }

                    var res = request.GetResponse();
                    _response = new StreamReader(res.GetResponseStream()).ReadToEnd();

                    _currentState = RunState.Done;

                    _shouldExpire = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
                catch (Exception ex)
                {
                    _response = ex.Message;
                    _currentState = RunState.Error;

                    _shouldExpire = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
            });
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC49C16-C595-4D9F-B486-D356C4671A47"); }
        }
    }
}