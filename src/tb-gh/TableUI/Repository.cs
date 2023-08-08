using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TableUI
{
    internal class Repository
    {
        private string _url;
        private int _expire;
        private string _auth;

        private string _response;

        public Repository(string url, int expire, string authorization)
        {
            _url = url;
            _expire = expire;
            _auth = authorization;
        }

        public string get()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Method = "GET";
                request.Timeout = _expire;

                if (_auth != null && _auth.Length > 0)
                {
                    System.Net.ServicePointManager.Expect100Continue = true;
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", _auth);
                }
                else
                {
                    request.Credentials = CredentialCache.DefaultCredentials;
                }

                var res = request.GetResponse();
                var responseStream = res.GetResponseStream();
                var reader = new StreamReader(responseStream);
                _response = reader.ReadToEnd();

                return _response;
            }
            catch (Exception ex)
            {
                _response = ex.Message;
                return _response;
            }
        }
    }
}
