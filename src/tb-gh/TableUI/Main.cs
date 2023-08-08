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

        private Repository _repository;
        private Parser _parser;

        public ParsedData _parsedData;

        public void setup(string url, int expire, string authorization)
        {
            _repository = new(url, expire, authorization);
        }

        public void run()
        {
            string jsonString = _repository.get();

            _parser = new Parser();
            _parsedData = _parser.Parse(jsonString);
        }

        public ParsedData get_results()
        {
            return _parsedData;
        }
    }
}
