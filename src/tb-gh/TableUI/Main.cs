﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    // This is the main class that will be called from the Grasshopper component
    internal class Main
    {
        private Repository repository;
        private Parser parser = new();

        public ParsedData parsedData;
        public ParsedData parsedDataHistory;
        public bool udpIsOpen;

        public void setup(string url, int expire, string authorization, string repoStrategy)
        {
            repository = Repository.Instance(url, expire, authorization);
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
            udpIsOpen = true;

            if (jsonString == null)
            {
                parsedData = null;
            } else
            {
                parser.Parse(jsonString);
                parsedData = parser.MakeDataList();
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
