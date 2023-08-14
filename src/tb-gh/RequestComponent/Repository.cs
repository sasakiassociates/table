using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiLogic
{
    public class Repository
    {
        private static Repository instance;
        private static readonly object padlock = new object();

        private Strategy repoStrategy;
        public string response;
        public List<Marker> parsedResponse;

        private Parser parser = new Parser();

        public static Repository Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Repository();
                    }
                    return instance;
                }
            }
        }

        public void Setup(string strategy)
        {
            if (strategy == "http")
            {
                SetStrategy<RepoHttpGet>();
            }
            else if (strategy == "udp")
            {
                SetStrategy<RepoUdpReceive>();
            }
            else
            {
                throw new Exception("Invalid repoStrategy");
            }
        }

        public void SetStrategy<T>() where T : Strategy
        {
            if (repoStrategy == null)
            {
                repoStrategy = RepoStrategy<T>.Instance;
            }
        }

        public string Response
        {
            get { return response; }
            set { response = value; }
        }

        public void MakeCall(string target, int timeout = 1000, string auth = "")
        {
            response = repoStrategy?.execute(target, timeout, auth);
        }

        public List<Marker> Get()
        {
            parsedResponse = parser.Parse(response);
            return parsedResponse;
        }
    }
}
