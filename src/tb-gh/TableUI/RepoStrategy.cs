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
    internal abstract class RepoStrategy
    {
        protected string response;

        public virtual string execute(string target, int timeout = 1000, string auth = "")
        {
            string result = "Error: strategy for the repo must be set";
            return result;
        }
    }

    internal class RepoStrategyHttpGet : RepoStrategy
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

    internal class RepoStrategyUdpReceive : RepoStrategy
    {
        private UdpClient udpClient;
        private int port = 5004;

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
            /*try
            {
                int port = int.Parse(target);
                UdpClient dpClient = new UdpClient(port);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = dpClient.Receive(ref remoteEP);

                response = Encoding.ASCII.GetString(data);
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return response;
            }*/
        }
    }
}
