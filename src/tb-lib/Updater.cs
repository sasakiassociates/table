using System;
using System.Collections.Generic;
using System.Text;

namespace TableLib
{
    // This class is a way to test if we can update a grasshopper component from a different thread
    public class Updater
    {
        private static Updater instance;
        private static readonly object _lock = new object();



        public static Updater Instance
        {
            get
            {
                // lock makes sure the Singleton works in a multi-threaded environment
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new Updater();
                    }
                    return instance;
                }
            }
        }

        public Updater() { }


    }
}
