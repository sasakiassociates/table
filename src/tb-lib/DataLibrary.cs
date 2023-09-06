using System;
using System.Collections.Generic;
using System.Text;

namespace TableLib
{
    public class DataLibrary
    {
        // this class holds persistant information for programs that cannot
        private static DataLibrary instance;
        private static readonly object _lock = new object();
        public Dictionary<int, object> refDict { get; set; }

        public static DataLibrary Instance
        {
            get
            {
                // lock makes sure the Singleton works in a multi-threaded environment
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new DataLibrary();
                    }
                    return instance;
                }
            }
        }

        private DataLibrary()
        {
            refDict = new Dictionary<int, object>();
        }

        public void BuildDict(int key, object incoming)
        {
            refDict.Add(key, incoming);
        }

        public object GetData(int key)
        {
            if (refDict.TryGetValue(key, out object value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
