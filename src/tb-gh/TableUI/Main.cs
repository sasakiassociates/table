using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    // This is the main class that will be called from the Grasshopper component
    internal class Main
    {
        private string _url;
        private string _auth;
        private int _expire;

        private Repository repository;
        private Parser parser = new();

        public ParsedData parsedData;

        public void setup(string url, int expire, string authorization, string repoStrategy)
        {
            repository = new(url, expire, authorization);
            if (repoStrategy == "HTTP")
            {
                repository.set_strategy(new RepoStrategyHttpGet());
            }
            else if (repoStrategy == "UDP")
            {
                repository.set_strategy(new RepoStrategyUdpReceive());
            }
            else
            {
                throw new Exception("Invalid repoStrategy");
            }
        }

        public void run()
        {
            string jsonString = repository.get();
            if (jsonString != null)
            {
                parser.Parse(jsonString);
                parsedData = parser.MakeDataList();
            } else
            {
                  throw new Exception("No data received");
            }
        }

        public ParsedData get_list_results()
        {
            return parsedData;
        }
    }
}
