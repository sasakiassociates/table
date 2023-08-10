using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiLogic
{
    // This is the main class that will be called from the Grasshopper component
    public class Main
    {
        private Repository repository;
        private Parser parser = new Parser();

        public ParsedData parsedData = new ParsedData();
        public ParsedData latestData;
        public bool udpIsOpen;

        public void setup(string url, int expire, string authorization, string repoStrategy)
        {
            repository = new Repository(url, expire, authorization);
            if (repoStrategy == "http")
            {
                repository.set_strategy<RepoStrategyHttpGet>();
            }
            else if (repoStrategy == "udp")
            {
                repository.set_strategy<RepoStrategyUdpReceive>();
            }
            else
            {
                throw new Exception("Invalid repoStrategy");
            }
        }

        public void run()
        {
            string jsonString = repository.get();

            if (jsonString != null && jsonString.Length > 0)
            {
                parser.Parse(jsonString);
                parsedData = parser.MakeDataList();
            }
            else if (latestData != null)
            {
                /*parsedData = latestData;*/
            }
        }

        public ParsedData get_list_results()
        {
            return parsedData;
        }

        public void closeUdp()
        {
            repository.close();
            udpIsOpen = false;
        }
    }
}
