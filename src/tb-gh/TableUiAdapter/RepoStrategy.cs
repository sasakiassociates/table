using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TableUiAdapter
{


    public abstract class Strategy
    {
        public abstract string execute(string target, int timeout = 1000, string auth = "");

        public abstract void End();
    }
  internal abstract class RepoStrategy<T> : Strategy where T : class
    {
        private static T instance;
        private static readonly object padlock = new object();
        protected string response;

        public static T Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = (T)Activator.CreateInstance(typeof(T));
                    }
                    return instance;
                }
            }
        }
  
    }

    internal class RepoHttpGet : RepoStrategy<RepoHttpGet>
    {
        public override string execute(string target, int timeout = 1000, string auth = "")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
                request.Method = "GET";
                request.Timeout = timeout;

                if (auth != null && auth.Length > 0)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    request.Headers.Add("Authorization", auth);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                reader.Close();
                response.Close();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public override void End()
        {
            return;
        }
    }

    internal class RepoUdpReceive : RepoStrategy<RepoUdpReceive>
    {
        private UdpClient udpClient;
        private int port = 5005;

        public override string execute(string target, int timeout = 1000, string auth = "")
        {
            try
            {
                if (udpClient == null)
                {
                    udpClient = new UdpClient(port);
                }
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                return returnData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public override void End()
        {
            udpClient.Close();
        }
    }
}
