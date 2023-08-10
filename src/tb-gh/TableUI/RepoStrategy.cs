using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal abstract class Strategy
    {
        public abstract string execute(string target, int timeout = 1000, string auth = "");
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

        public override string execute(string target, int timeout = 1000, string auth = "")
        {
            string result = "Error: strategy for the repo must be set";
            return result;
        }
    }

    internal class RepoStrategyHttpGet : RepoStrategy<RepoStrategyHttpGet>
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

                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", auth);
                }
                else
                {
                    request.Credentials = CredentialCache.DefaultCredentials;
                }

                var res = request.GetResponse();
                var responseStream = res.GetResponseStream();
                var reader = new StreamReader(responseStream);
                response = reader.ReadToEnd();

                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return response;
            }
        }
    }

    internal class RepoStrategyUdpReceive : RepoStrategy<RepoStrategyUdpReceive>
    {
        private UdpClient udpClient;
        private int port = 5005;

        public override string execute(string target, int timeout = 1000, string auth = "")
        {
            udpClient ??= new UdpClient(port);

            if (udpClient.Available > 0)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] receivedBytes = udpClient.Receive(ref endPoint);
                string receivedData = System.Text.Encoding.UTF8.GetString(receivedBytes);
                return receivedData;
            }
            else
            {
                return null;
            }
        }

        public void close()
        {
            udpClient.Close();
        }
    }
}
